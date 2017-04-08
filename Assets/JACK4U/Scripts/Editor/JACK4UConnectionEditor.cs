/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using PortAudioSharp;


namespace JACK4U{

	[CustomEditor(typeof(JACK4UConnection))]
	public class JACK4UConnectionEditor : Editor {


		private static Texture2D _tex;
		private Texture2D _tex_logo;
		private static GUIStyle style;
		private JACK4UConnection _target;
		private string[] _options;
		private int _portIndex = 0;
		private static Dictionary<string,PortAudio.PaHostApiInfo>  apiDict;


		SerializedProperty  AutoConnect_Prop;
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
		SerializedProperty  numDSPBuffers_Prop;
#endif
		SerializedProperty  inputChannels_Prop;
		SerializedProperty  outputChannels_Prop;



		public static void GetAvailableAPIs(){
			Audio.LoggingEnabled = true;
			apiDict = PortAudioSharp.Audio.GetAvailableAPIs();
		}



		public static void Init(){
			if(_tex == null) _tex = JACK4UUtils.MakeTexture(2,2, new Color(0.95f,0.95f,0.95f,1.0f));
		}

		void OnEnable(){

			if(target  !=_target) _target = target as JACK4UConnection;
			serializedObject.Update();
			AutoConnect_Prop = serializedObject.FindProperty("m_AutoConnect");
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			numDSPBuffers_Prop = serializedObject.FindProperty("numDSPBuffers");
#endif
			inputChannels_Prop = serializedObject.FindProperty("inputChannels");
			outputChannels_Prop = serializedObject.FindProperty("outputChannels");

			serializedObject.ApplyModifiedProperties();

			_tex_logo = Resources.Load(JACK4UUtils.LOGO32_NAME,typeof(Texture2D)) as Texture2D;

			Init();
		}

		void OnDisable(){

		}


		override public void OnInspectorGUI(){
		
			GUILayout.Space(5);
			if(_tex_logo != null){
				JACK4UUtils.DrawClickableTextureHorizontal(_tex_logo,()=>{EditorApplication.ExecuteMenuItem(JACK4UUtils.MENUITEM_EDITOR);});
			}

			if(JACK4UConnection.instance != _target){
				DrawConnectionInstanceError();
				return;
			}

			style = new GUIStyle(GUI.skin.box);
			style.normal.background =_tex;
			style.normal.textColor = Color.white;
			style.margin = new RectOffset(0,0,0,2);

			serializedObject.Update();

			EditorGUIUtility.LookLikeControls(150f,50f);
			//DrawDefaultInspector();

            EditorGUILayout.PropertyField(AutoConnect_Prop, new GUIContent("AutoConnect on Start", "AutoConnect on Start"));
			GUILayout.Space(10);
			DrawApiPopup();
			#region AudioSettings
			EditorGUI.BeginChangeCheck();

			JACK4UConnection.SampleRates newSampleRate = (JACK4UConnection.SampleRates)EditorGUILayout.EnumPopup("Sample Rate",_target.sampleRate);
			if(newSampleRate != _target.sampleRate){
				Undo.RecordObject(_target, "Set Sample Rate");
				_target.sampleRate = newSampleRate;
				EditorUtility.SetDirty(_target);
			}

		
			//as EditorGUILayout.PropertyField shows the enum with underscores we use this version:
            JACK4UConnection.fpb newFpb = (JACK4UConnection.fpb)EditorGUILayout.EnumPopup(new GUIContent("Frames Per Buffer", "Frames Per Buffer. Should match with the settings in QJackctl."), _target.framesPerBuffer);
			if(newFpb != _target.framesPerBuffer){
				Undo.RecordObject(_target, "Set Frames Per Buffer");
				_target.framesPerBuffer = newFpb;
				EditorUtility.SetDirty(_target);
			}
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
			GUILayout.Space(10);

			JACK4UConnection.dspbfs newDspbfs = (JACK4UConnection.dspbfs)EditorGUILayout.EnumPopup("DSP Buffer Size",_target.DSPBufferSize);
			if(newDspbfs != _target.DSPBufferSize){
				Undo.RecordObject(_target, "Set DSP Buffer Size");
				_target.DSPBufferSize = newDspbfs;
				EditorUtility.SetDirty(_target);
			}

			EditorGUILayout.PropertyField(numDSPBuffers_Prop,new GUIContent("DSP Buffers","test"));
			numDSPBuffers_Prop.intValue = Math.Max(2,numDSPBuffers_Prop.intValue);
#endif

			if(EditorGUI.EndChangeCheck()){
				//EditorUtility.SetDirty(_target);
				serializedObject.ApplyModifiedProperties();
				_target.enabled = ! _target.enabled;
				_target.enabled = ! _target.enabled;
			}

			#endregion



			GUILayout.Space(10);

            EditorGUILayout.PropertyField(inputChannels_Prop, new GUIContent("Input Channels", "Input Channels amount. At the moment we only support two channels."));
            EditorGUILayout.PropertyField(outputChannels_Prop, new GUIContent("Output Channels", "Output Channels. Doesn't make really sense but you should have the freedom to route the Audio out via JACK."));



			GUILayout.Space(10);


			if(Application.isPlaying){
				JACK4UEditor.CreateStyle();
				JACK4UEditor.DrawStartJackAudioButton();
			}

			DrawConnectionStatus();

			serializedObject.ApplyModifiedProperties();
		
		}

		private void DrawApiPopup(){

			if(apiDict== null)return;
			if(apiDict.Keys== null)return;
			if(apiDict.Keys.Count == 0)return;

			_options = apiDict.Keys.ToArray();

			_portIndex = Array.FindIndex(_options,item => item == _target.hostApiName);
			_portIndex = Mathf.Max(0,_portIndex);
			_portIndex = EditorGUILayout.Popup("Audio API", _portIndex, _options);

			_target.hostApiName = _options[_portIndex];
		}

	

		void DrawConnectionStatus ()
		{

			GUILayout.BeginVertical("box");

			GUILayout.BeginHorizontal();
			GUILayout.Label("PortAudio Messages:");
			GUILayout.FlexibleSpace();

			GUILayout.EndHorizontal();

			GUIContent gc = new GUIContent("","");
			Rect area;

		
			GUILayout.BeginHorizontal("box");

			GUILayout.BeginVertical(GUILayout.Height(50f),GUILayout.Width(50f));
			GUILayout.FlexibleSpace();
			GUILayout.Label("Status:");
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();

			gc.text = JACK4UConnection.statusMsg;
			area = GUILayoutUtility.GetRect(gc,style,GUILayout.MinHeight(40f),GUILayout.ExpandWidth(true));// (195.0f, 80.0f);
			area.width*=1f;
			EditorGUI.HelpBox(area,gc.text, MessageType.None);

			GUILayout.EndHorizontal();

			GUI.backgroundColor =  Color.white;

			GUILayout.BeginHorizontal("box");

			GUILayout.BeginVertical(GUILayout.Height(50f),GUILayout.Width(50f));
			GUILayout.FlexibleSpace();
			GUILayout.Label("Error:");
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();


			//GUI.backgroundColor = String.IsNullOrEmpty(PAConnection.errorMsg) ? Color.clear : Color.white;

			gc.text = JACK4UConnection.errorMsg;
			area = GUILayoutUtility.GetRect(gc,style,GUILayout.MinHeight(40f),GUILayout.ExpandWidth(true));// (195.0f, 80.0f);
			area.width*=1f;
			EditorGUI.HelpBox(area,gc.text, MessageType.None);
			GUILayout.EndHorizontal();


			GUILayout.EndVertical();

		}


		void DrawConnectionInstanceError(){
			string msg = "There should be just one JACK4U Connection instance in your scene! This GameObject will be destroyed in play mode!";
				Rect area = GUILayoutUtility.GetRect (195.0f, 40.0f);
				EditorGUI.HelpBox(area,msg,MessageType.Error);
		}

	}
}
