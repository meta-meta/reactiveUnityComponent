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
using PortAudioSharp;


namespace JACK4U{
	
	public class JACK4UAudio  {


		public static System.Object lockObj = new System.Object();
		public static bool isRunning{get{return _isRunning;}}

		public const int BUFFERSIZE = 8192;
		public delegate void statusEventHandler(object sender, string msg);
		public static event statusEventHandler OnError;
		public static event statusEventHandler OnStatus;


		public RingQueue<float> bufferRingQueue = new RingQueue<float>(BUFFERSIZE);


		private string  _hostApiName;
		private int _sampleRate;
		private int _framePerBuffer = 64;
		
		private int _inputChannels = 2;
		private int _outputChannels = 0;

		private PortAudioSharp.Audio pas_audio;
		private float[] callbackBuffer = new float[BUFFERSIZE];

		private int frameCountTwice;
		private bool _isSetup;
		private static bool _isRunning;

		//singelton
		private  static JACK4UAudio _instance;

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public static JACK4UAudio Instance {
			get{
				if(_instance == null) _instance = new JACK4UAudio();
				return _instance;
			}

		}

		private JACK4UAudio(){

		}


		public void SetUp(int sampleRate, int inputChannels, int outputChannels,int framePerBuffer, string hostApiName){
			_sampleRate = sampleRate;
			_inputChannels = inputChannels;
			_outputChannels = outputChannels;
			_framePerBuffer = framePerBuffer;
			_hostApiName = hostApiName;
			_isSetup= true;
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start(){

			pas_audio = null;

			Audio.OnStatus-=OnAudioStatus;
			Audio.OnStatus+=OnAudioStatus;
			try {
				if(!_isSetup){
					throw new Exception("Audio is not setup");
				}
		
				Audio.LoggingEnabled = true;

				pas_audio = new PortAudioSharp.Audio(_inputChannels, _outputChannels, _sampleRate, (uint)_framePerBuffer, new PortAudio.PaStreamCallbackDelegate(myPaStreamCallback),_hostApiName);
				pas_audio.Start();
				_isRunning = true;
				if(OnStatus != null) OnStatus(this, "Connected to JACK via "+_hostApiName);

			} catch(Exception e) {
				_isRunning = false;
				if(OnError != null) OnError(this, e.Message);

			} 

		}


		private void OnAudioStatus(string msg){
			if(OnStatus != null) OnStatus(this, msg);
		}

		private PortAudio.PaStreamCallbackResult myPaStreamCallback(
			IntPtr input,
			IntPtr output,
			uint frameCount, 
			ref PortAudio.PaStreamCallbackTimeInfo timeInfo,
			PortAudio.PaStreamCallbackFlags statusFlags,
			IntPtr userData)
		{

			frameCountTwice = (int)frameCount *2;//inputChannels;
			lock(lockObj){
			try {

				if (callbackBuffer.Length < frameCountTwice) callbackBuffer = new float[frameCountTwice];

				Marshal.Copy(input, callbackBuffer, 0, frameCountTwice);
				if(output != IntPtr.Zero)Marshal.Copy(callbackBuffer,0,output,  frameCountTwice);//route out directly

				for(var i = 0;i<frameCountTwice;i++){
					bufferRingQueue.Write(callbackBuffer[i]);
				}
				
			} catch (Exception e) { 
				if(OnError != null) OnError(this,e.Message);
			}
			
			return PortAudio.PaStreamCallbackResult.paContinue;

			}
		}


		public void Stop(){
			if(pas_audio != null){
				pas_audio.Stop();
				pas_audio.Dispose();
				lock(lockObj){
					bufferRingQueue.Clear();
				}
				_isRunning = false;
				if(OnStatus != null) OnStatus(this, "Stopped connection to JACK");
				Audio.OnStatus-=OnAudioStatus;

			}
		}



	}

}
