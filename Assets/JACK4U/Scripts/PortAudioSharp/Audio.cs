 /*
  * PortAudioSharp - PortAudio bindings for .NET
  * Copyright 2006-2011 Riccardo Gerosa and individual contributors as indicated
  * by the @authors tag. See the copyright.txt in the distribution for a
  * full listing of individual contributors.
  * Changes for Unity and better status&error management by Stefan Schlupek 
  *
  * Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
  * and associated documentation files (the "Software"), to deal in the Software without restriction, 
  * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
  * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
  * subject to the following conditions:
  *
  * The above copyright notice and this permission notice shall be included in all copies or substantial 
  * portions of the Software.
  *
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT 
  * NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
  * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
  * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
  * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
  */

using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace PortAudioSharp {

	/**
		<summary>
			A simplified high-level audio class
		</summary>
	*/
	public class Audio : IDisposable {
	
		private int inputChannels, outputChannels, frequency;
		private uint framesPerBuffer;
		private PortAudio.PaStreamCallbackDelegate paStreamCallback;
		private int hostApi;
		private PortAudio.PaHostApiInfo apiInfo;
		private PortAudio.PaDeviceInfo inputDeviceInfo, outputDeviceInfo;
		private IntPtr stream;
		private static bool loggingEnabled = false;
	 	private bool disposed = false;

		private  int jackRouter = -1;

		public delegate void statusEventHandler(string msg);
		//public static event statusEventHandler OnError;
		public static event statusEventHandler OnStatus;
	 	
	 	public static bool LoggingEnabled {
	 		get { return loggingEnabled; }
	 		set { loggingEnabled = value; }
	 	}

		//for unity dropdown. Should not be called in arunning PortAudio Session as we shutDown PortAudio!
		public static Dictionary<string,PortAudio.PaHostApiInfo> GetAvailableAPIs(){
			if (errorCheck("Initialize",PortAudio.Pa_Initialize())) {
				//this.disposed = true; 
				// if Pa_Initialize() returns an error code, 
				// Pa_Terminate() should NOT be called.
				throw new Exception("Can't initialize PortAudio");
			}
			var dict = new Dictionary<string,PortAudio.PaHostApiInfo> ();

			int apiCount = PortAudio.Pa_GetHostApiCount();
			for (int i = 0; i < apiCount; i++) {
				PortAudio.PaHostApiInfo availableApiInfo = PortAudio.Pa_GetHostApiInfo(i);
				dict.Add(availableApiInfo.name,availableApiInfo);
				log("available API index: " + i + "\n" + availableApiInfo.name.ToString());
			}
			//as we call this method at startup time we have to shutdown PortAudio again otherwise the JackRouter will not be available if jackd wasn't already running.
			log("Terminating...");
			if(errorCheck("Terminate",PortAudio.Pa_Terminate()) ){
				throw new Exception("Can't terminate PortAudio");
			}
			return dict;
		}



		public Audio(int inputChannels, int outputChannels, int frequency, uint framesPerBuffer,
		    PortAudio.PaStreamCallbackDelegate paStreamCallback,string apiName) {
	
			log("Initializing...");
			this.inputChannels = inputChannels;
			this.outputChannels = outputChannels;
			this.frequency = frequency;
			this.framesPerBuffer = framesPerBuffer;
			this.paStreamCallback = paStreamCallback;

			if (errorCheck("Initialize",PortAudio.Pa_Initialize())) {
				this.disposed = true; 
				// if Pa_Initialize() returns an error code, 
				// Pa_Terminate() should NOT be called.
				throw new Exception("Can't initialize PortAudio");
			}

			this.hostApi = _getApiIndexFromApiName(apiName);
			if(this.hostApi <0){
				throw new Exception("Error: Could not select HostAPI "+apiName);
			}

			this.apiInfo = PortAudio.Pa_GetHostApiInfo(this.hostApi);
			log("selected Host API: " + this.apiInfo.ToString());
			this.inputDeviceInfo = PortAudio.Pa_GetDeviceInfo(apiInfo.defaultInputDevice);
			this.outputDeviceInfo = PortAudio.Pa_GetDeviceInfo(apiInfo.defaultOutputDevice);

			this.jackRouter = -1;
			for(int i = 0;i<this.apiInfo.deviceCount;i++){
				int deviceIndex = PortAudio.Pa_HostApiDeviceIndexToDeviceIndex(this.hostApi, i);
				var deviceInfo = PortAudio.Pa_GetDeviceInfo(deviceIndex);
				//log("JackRouter "+i+":\n" + deviceInfo.ToString());
				if(deviceInfo.name == "JackRouter") this.jackRouter = PortAudio.Pa_HostApiDeviceIndexToDeviceIndex(this.hostApi, i);
			}
			//0: ASIO4ALL v2 
			//1: JackRouter
			//log("JackRouter :\n" + jackRouter.ToString());
			if(this.jackRouter <0){
				throw new Exception("No JackRouter Available");
			}

			this.inputDeviceInfo = PortAudio.Pa_GetDeviceInfo(this.jackRouter);
			this.outputDeviceInfo = PortAudio.Pa_GetDeviceInfo(this.jackRouter);

//			Debug.Log("********************");
//			log("input device:\n" + inputDeviceInfo.name.ToString());
//			log("output device:\n" + outputDeviceInfo.name.ToString());
//			Debug.Log("********************");
			
		}

	 	
	 	public void Start() {
	 		log("Starting...");
			if(this.jackRouter <0){
				throw new Exception("No JackRouter Available");
			}
	 
			this.stream = streamOpen(this.jackRouter, this.inputChannels, this.jackRouter, this.outputChannels, this.frequency, this.framesPerBuffer);
			//log("Stream pointer: " + stream.ToInt32());
			streamStart(stream);
	 	}
	 	
	 	public void Stop() {
	 		//log("Stopping...");
	 		streamStop(this.stream);
			streamClose(this.stream);
			this.stream = new IntPtr(0);
	 	}
			
	 	private static void log(String logString) {
	 		if (loggingEnabled)  
				Debug.Log("PortAudio: " + logString);
			if(OnStatus != null) OnStatus( logString);
	 	} 
	 	
	 	private static bool errorCheck(String action, PortAudio.PaError errorCode) {
	 		if (errorCode != PortAudio.PaError.paNoError) {
	 			log(action + " error: " + PortAudio.Pa_GetErrorText(errorCode));
	 			if (errorCode == PortAudio.PaError.paUnanticipatedHostError) {
	 				PortAudio.PaHostErrorInfo errorInfo = PortAudio.Pa_GetLastHostErrorInfo();
	 				log("- Host error API type: " + errorInfo.hostApiType);
	 				log("- Host error code: " + errorInfo.errorCode);
	 				log("- Host error text: " + errorInfo.errorText);
	 			}
	 			return true;
	 		} else {
	 			log(action + " OK");
	 			return false;
	 		}
	 	}
	 	
	 	private int apiSelect() {
			int selectedHostApi = PortAudio.Pa_GetDefaultHostApi();
			int apiCount = PortAudio.Pa_GetHostApiCount();
			for (int i = 0; i<apiCount; i++) {
				PortAudio.PaHostApiInfo apiInfo = PortAudio.Pa_GetHostApiInfo(i);

                if ((apiInfo.type == PortAudio.PaHostApiTypeId.paASIO)){
					selectedHostApi = i;
					break;
				}
			}
			return selectedHostApi;
		}

		private int _getApiIndexFromApiTypeId(PortAudio.PaHostApiTypeId typeId){
			int selectedHostApi = -1;
			int apiCount = PortAudio.Pa_GetHostApiCount();
			for (int i = 0; i<apiCount; i++) {
				PortAudio.PaHostApiInfo apiInfo = PortAudio.Pa_GetHostApiInfo(i);
				if (apiInfo.type == typeId){
					selectedHostApi = i;
					break;
				}
			}
			return selectedHostApi;
		}

		private int _getApiIndexFromApiName(string apiName){
			int selectedHostApi = -1;
			int apiCount = PortAudio.Pa_GetHostApiCount();
			for (int i = 0; i<apiCount; i++) {
				PortAudio.PaHostApiInfo apiInfo = PortAudio.Pa_GetHostApiInfo(i);
				if (apiInfo.name == apiName){
					selectedHostApi = i;
					break;
				}
			}
			return selectedHostApi;
		}

		
	 	private IntPtr streamOpen(int inputDevice,int inputChannels,
                         int outputDevice,int outputChannels,
                         int sampleRate, uint framesPerBuffer) {

	 		IntPtr stream = new IntPtr();
	 		IntPtr data = new IntPtr(0);
            
	 		PortAudio.PaStreamParameters? inputParams;
            if (inputDevice == -1 || inputChannels <= 0) {
                inputParams = null;
            } else {
                PortAudio.PaStreamParameters inputParamsTemp = new PortAudio.PaStreamParameters();
                inputParamsTemp.channelCount = inputChannels;
                inputParamsTemp.device = inputDevice;
                inputParamsTemp.sampleFormat = PortAudio.PaSampleFormat.paFloat32;
                inputParamsTemp.suggestedLatency = this.inputDeviceInfo.defaultLowInputLatency;
                inputParams = inputParamsTemp;
            }
            PortAudio.PaStreamParameters? outputParams; 
            if (outputDevice == -1 || outputChannels <= 0) {
                outputParams = null;
            } else {
                PortAudio.PaStreamParameters outputParamsTemp = new PortAudio.PaStreamParameters();
	 		    outputParamsTemp.channelCount = outputChannels;
	 		    outputParamsTemp.device = outputDevice;
	 		    outputParamsTemp.sampleFormat = PortAudio.PaSampleFormat.paFloat32;
	 		    outputParamsTemp.suggestedLatency = this.outputDeviceInfo.defaultLowOutputLatency;
                outputParams = outputParamsTemp;
            }

	 		bool lastError = errorCheck("OpenDefaultStream",PortAudio.Pa_OpenStream(
			    out stream,
                ref inputParams,
                ref outputParams,
			    sampleRate,
			    framesPerBuffer,
			    PortAudio.PaStreamFlags.paNoFlag,
			    this.paStreamCallback,
			    data)); 
			if(lastError) throw new Exception("Error opening a PortAudio Stream! Look at the console for further info.");
			return stream;
		}
	 	
		
		private void streamClose(IntPtr stream) {
			errorCheck("CloseStream",PortAudio.Pa_CloseStream(stream));
		}
		
		private void streamStart(IntPtr stream) {
			errorCheck("StartStream",PortAudio.Pa_StartStream(stream));
		}
		
		private void streamStop(IntPtr stream) {
			errorCheck("StopStream",PortAudio.Pa_StopStream(stream));
		}
		
		/*
		private void streamWrite(IntPtr stream, float[] buffer) {
			errorCheck("WriteStream",PortAudio.Pa_WriteStream(
				stream,buffer,(uint)(buffer.Length/2)));
		}
		*/
   
        private void Dispose(bool disposing)
        {
            if(!this.disposed)
            {
                if(disposing)
                {
                	// Dispose here any managed resources
                }
             
             	// Dispose here any unmanaged resources
                log("Terminating...");
	 			errorCheck("Terminate",PortAudio.Pa_Terminate());
            }
            this.disposed = true;         
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        ~Audio() {
	 		Dispose(false);
	 	}
	}

}
