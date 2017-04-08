/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System;

namespace JACK4U{

	[AddComponentMenu("JACK4U/JackGUI")]
	[ExecuteInEditMode]
	public class JACK4UGui : MonoBehaviour {

		#region public
		public bool ShowInEditMode;
		public static string[] selStrings = new string[] {"16", "32", "64", "128","256","512","1024","2048","4096"};

		#endregion

		#region private
		private bool _showGUI= true;
		private JACK4UConnection pcon;
		private int selGridInt = 0;
		public static Texture2D _tex_logo;

		#endregion
	
		void Start () {
		
		}
		void OnEnable(){
			_tex_logo = Resources.Load(JACK4UUtils.JACK32_NAME,typeof(Texture2D)) as Texture2D;
		}
		void OnDisable(){

		}
	
		void Update () {
		
		}

		void OnGUI(){
			if(!Application.isPlaying){
				if(!ShowInEditMode)return;
			}


			GUILayout.BeginVertical(GUILayout.Width(Screen.width),GUILayout.Height(Screen.height));
			GUILayout.Space(5f);



			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			_showGUI = GUILayout.Toggle(_showGUI,new GUIContent(_showGUI? "Hide GUI":"Show GUI"),GUI.skin.button,GUILayout.Height(30f),GUILayout.Width(100f));
			GUILayout.Space(5f);
			GUILayout.EndHorizontal();
			if(!_showGUI || JACK4UConnection.instance == null){
				if(JACK4UConnection.instance == null) GUILayout.Label("No Connection is available in your scene:",GUILayout.Height(30f));
				GUILayout.FlexibleSpace();
				DrawLogo();
				GUILayout.Space(10f);

				GUILayout.EndVertical();

				return;
			}

			pcon = JACK4UConnection.instance;

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawStartPortAudioButton();
			GUILayout.Space(5f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Host Api Name:",GUILayout.Height(30f));
			GUILayout.Label(pcon.hostApiName,GUILayout.Height(30f));
			GUILayout.Space(5f);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Frames per Buffer:",GUILayout.Height(30f));
			GUILayout.Label(Convert.ToString((int)pcon.framesPerBuffer),GUILayout.Height(30f));
			GUILayout.Space(5f);
			GUILayout.EndHorizontal();

			if(!JACK4UAudio.isRunning && Application.isPlaying){
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.BeginHorizontal(GUILayout.Width(120f));

				selGridInt = (int)Mathf.Log((float)JACK4UConnection.instance.framesPerBuffer,2f) -4;
				selGridInt = GUILayout.SelectionGrid(selGridInt,selStrings,3);
				JACK4UConnection.instance.framesPerBuffer =(JACK4UConnection.fpb)( Mathf.Pow( 2,(4+ selGridInt)));

				GUILayout.Space(5f);
				GUILayout.EndHorizontal();
				GUILayout.EndHorizontal();
			}

			GUILayout.FlexibleSpace();
			DrawLogo();

			GUILayout.Space(10f);

			GUILayout.EndVertical();
		}


		public static void DrawLogo(){

						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						JACK4UUtils.DrawTexture(_tex_logo);
						GUILayout.FlexibleSpace();
						GUILayout.EndHorizontal();
		}

		public static void DrawStartPortAudioButton ()
		{
			GUIContent gc = new GUIContent("","");
			gc.text = JACK4UAudio.isRunning ? "Disconnet" : "Connect";
			if( GUILayout.Button(gc,GUILayout.Height(30f),GUILayout.Width(100f))){
				if(JACK4UAudio.isRunning){
					JACK4UConnection.instance.StopPortAudio();
				}else{ 
					JACK4UConnection.instance.StartPortAudio();
				}
			}
		}


	}
}
