/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System;  
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;


using PortAudioSharp;

namespace JACK4U{

	#pragma warning disable 168
	#pragma warning disable 414
	[AddComponentMenu("JACK4U/Jack Connection")]
	[ExecuteInEditMode]
	[RequireComponent(typeof(AudioSource))]
	//[RequireComponent(typeof(AudioListener))]
	public class JACK4UConnection : MonoBehaviour
	{
		public static string errorMsg;
		public static string statusMsg;
		public static bool IsProcessElevated;//admin priviliges?
		public static bool IsUacEnabled;

		public bool m_AutoConnect = true;
		public string  hostApiName = "ASIO";
		public dspbfs DSPBufferSize= dspbfs._1024;
		public int numDSPBuffers = 2;
		public Channels inputChannels = Channels.two;
		public Channels outputChannels = Channels.none;
		public fpb framesPerBuffer = fpb._64;
		public SampleRates sampleRate = SampleRates._44100;

		public static JACK4UConnection instance
		{
			get
			{
				if(_instance == null){
					_instance = GameObject.FindObjectOfType<JACK4UConnection>();
					if(_instance == null) return null;
					DontDestroyOnLoad(_instance.gameObject);
				}

				return _instance;
			}
		}


		#region enum
		public enum dspbfs {
			_128=128,_256=256,_512=512,_1024=1024,_2048=2048
		} ; 

		public enum fpb {
			_16=16,_32=32,_64=64,_128=128,_256=256,_512=512,_1024=1024,_2048=2048,_4096=4096
		} ; 

		public enum SampleRates {
			_22050=22050,_32000=32000,_44100=44100,_48000=48000,_88200=88200,_96000=96000,_192000=192000
		} ;

		public enum Channels {
			none=0,two=2
		} ; 
		#endregion enum

		#region private
		private static JACK4UConnection _instance;
		[SerializeField,HideInInspector]
		private bool redrawFlag;
		private int redrawCount = 0;

		private JACK4UAudio _j4aAudio;
		private int bufferLength, numBuffers;

		private AudioSource _audioSource;
	
		#endregion

	

		public static void SetProcessElevation(){
			IsUacEnabled = UacHelper.IsUacEnabled;
			IsProcessElevated = UacHelper.IsProcessElevated;
		}

		private static void OnErrorReceived(object sender, string msg){
			errorMsg=msg;
		}
		
		private static void OnStatusReceived(object sender, string msg){
            Debug.Log(msg);
			statusMsg=msg;
		}

		private static void _ResetMsgFields(){
			errorMsg = String.Empty;
			statusMsg = String.Empty;
		}


		void Awake(){

			if(Application.isPlaying){

				if(instance != this)
				{
					Destroy(this.gameObject);
				}

			}//isPlaying
		
			SetProcessElevation();
		}


	

		void OnEnable(){

			_UpdateDSPBufferSize();

			#if UNITY_EDITOR
			if(!Application.isPlaying){
				UnityEditor.EditorApplication.update -= _Update;
				UnityEditor.EditorApplication.update += _Update;
			}
			#endif

			_ResetMsgFields();

			JACK4UAudio.OnError-=OnErrorReceived ;//security
			JACK4UAudio.OnError+=OnErrorReceived ;
			
			JACK4UAudio.OnStatus-=OnStatusReceived ;//security
			JACK4UAudio.OnStatus+=OnStatusReceived ;

			//little hack to initialize the AudioSource, otherwise we hear nothing when the AudioListener is on another GameObject
			_audioSource = GetComponent<AudioSource>();
			if(_audioSource != null){
			_audioSource.enabled = false;
			_audioSource.enabled = true;
			}

		}

		private void _UpdateDSPBufferSize(){
            //Has to be modified for Unity 5 
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
            AudioSettings.SetDSPBufferSize ((int)DSPBufferSize,numDSPBuffers);
            AudioSettings.outputSampleRate = (int)sampleRate;
#else          
           // AudioConfiguration ac = AudioSettings.GetConfiguration();
            //Debug.Log("<color='red'>dspBufferSize:" + ac.dspBufferSize + "</color>");
#endif
            		
			AudioSettings.GetDSPBufferSize(out bufferLength,out numBuffers);
			
		}

		void Start(){
			if(!Application.isPlaying)return;
			if(!m_AutoConnect)return;
			_startPortAudio();
		}

		public void StartPortAudio(){
			_startPortAudio();
		}

		private void _startPortAudio(){
		
			if(!Application.isPlaying)return;
			_j4aAudio = JACK4UAudio.Instance;
			if(_j4aAudio == null)return;
			if(JACK4UAudio.isRunning)return;

			_ResetMsgFields();

			_j4aAudio.SetUp((int)sampleRate,(int)inputChannels,(int)outputChannels,(int)framesPerBuffer,hostApiName);
			_j4aAudio.Start();
		}



		void Update(){
			_Update();
		}

		void _Update(){
			//better live update of GUI but tooltips become problems.
			redrawCount++;
			if(redrawCount>9)redrawFlag = ! redrawFlag;
			redrawCount%=10;
		}

		void OnDisable(){
	
			#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= _Update;
			#endif
			Stop ();
		}

		void OnDestroy(){
			if(_instance == this)_instance = null;
		}

		void OnApplicationQuit(){
			Stop();
			Thread.Sleep(100);
		}

		void Stop(){
			_StopPortAudio();
			JACK4UAudio.OnError-=OnErrorReceived ;
			JACK4UAudio.OnStatus-=OnStatusReceived ;
			
		}

		public void StopPortAudio(){
			_StopPortAudio();
		}
		private void _StopPortAudio(){

			if(_j4aAudio != null){
				_j4aAudio.Stop();
				//_paAudio.Dispose();
				
			}
		}

		/// <summary>
		/// Raises the audio filter read event. Here we read the data from the Portaudio buffers and inject them into the Unity audio system.
		/// </summary>
		/// <param name="data">Data.</param>
		/// <param name="channels">Channels.</param>
		void OnAudioFilterRead(float[] data, int channels)
		{
			if(_j4aAudio == null)return;

			//bufferLength*2 == data.length
			lock(JACK4UAudio.lockObj){

				for(var i = 0; i < bufferLength*2; i = i + (channels)){
						
						try{
						data[i] = _j4aAudio.bufferRingQueue.Read();

						}catch(Exception e){
							data[i]=0f;
						}
						

						try{
							if (channels == 2) {
							data[i+1]=_j4aAudio.bufferRingQueue.Read();
							}

						}catch(Exception e){
							data[i+1]=0f;
						}
						
				}//for



			}//lock

		}


	#pragma warning restore 168
	#pragma warning restore 414


	}

} 