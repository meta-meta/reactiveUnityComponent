/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;
using monoflow;
using System.Runtime.InteropServices;
using System.Linq;



namespace JACK4U {

	public class JACK4UUtils  {

		#region OSCheck
		public static bool is64BitProcess = (IntPtr.Size == 8);
		public static bool is64BitOperatingSystem = is64BitProcess || InternalCheckIsWow64();
		
		public static bool InternalCheckIsWow64()
		{
			if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
			    Environment.OSVersion.Version.Major >= 6)
			{
				using (Process p = Process.GetCurrentProcess())
				{
					bool retVal;
					if (!NativeMethods.IsWow64Process(p.Handle, out retVal))
					{
						return false;
					}
					return retVal;
				}
			}
			else
			{
				return false;
			}
		}
		#endregion

		public const string  MENUITEM_ROOT = "Window/JACK4U/";
		public const string  MENUITEM_EDITOR = MENUITEM_ROOT+"Editor";
		public const string  MENUITEM_CREATE_CONNECTION = "GameObject/Create Other/JACK4U/Connection";
		public const string CONFIGPATH_EDITOR ="Resources/config.asset";

		public const string  MENUITEM_SET_QJACKCTL_PATH = MENUITEM_ROOT+"Set Path To qjackctl.exe";
		public const string  MENUITEM_SET_DAW_PATH = MENUITEM_ROOT+"Set Path To DAW exe";
		public const string  MENUITEM_SET_JACKD_PATH = MENUITEM_ROOT+"Set Path To Jackd.exe";
		public const string QJACKCTL_NAME = "qjackctl";


		public const string LOGO32_NAME = "logo.logotype.128x32";
		public const string JACK32_NAME = "logo.128x32";


		#region Process

		public static void RestartCurrentProcess(bool asAdmin){

			Process p = StartProcess(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName,asAdmin);
			if(p == null)return;
			Environment.Exit(-1);//Force termination of the current process.
		}


	

		public static Process StartProcess(string path,bool asAdmin){

			ProcessStartInfo si = new ProcessStartInfo();
			si.FileName = path;
			if(asAdmin) si.Verb ="runas";


			try{
				Process p= Process.Start(si);
				p.EnableRaisingEvents = true;
				return p;

			}catch{
//				UnityEngine.Debug.Log("Error starting process.");
				return null;
			}
		}


		public static bool IsProcessOpen(string name){
	
			//return Process.GetProcesses().Where(p => p.ProcessName.Contains(name)).Count() >= 1  ;

			foreach (Process clsProcess in Process.GetProcesses()) {

			try{
					if(clsProcess == null)continue;
					if (clsProcess.ProcessName.Contains(name))return true;
				}catch (Exception){
					return false;
				}
			}//for
			return false;
		}


		public static int GetProcessIdByName(string name){
		
			foreach (Process clsProcess in Process.GetProcesses()) {	
				try{
					if(clsProcess == null)continue;
					if (clsProcess.ProcessName.Contains(name))return clsProcess.Id;
				}catch (InvalidOperationException){
					//next process
				}
			}//for
			return 0;
		}

				

		public static bool IsWindowOpen(string name)
		{
	
			bool result = false;

			monoflow.NativeMethods.EnumWindowsProc callback =  (IntPtr hWnd, IntPtr lParam)=> { 
				int length = monoflow.NativeMethods.GetWindowTextLength(hWnd);
				StringBuilder sb = new StringBuilder(length + 1);
				int result2 = monoflow.NativeMethods.GetWindowText(hWnd, sb, sb.Capacity);
				if (result2 > 0) {

					if(sb.ToString() == name) {
						result = true;
						return false;//stop callback call
					}
				}
				return true;
				} ;

			monoflow.NativeMethods.EnumWindows(callback, IntPtr.Zero);

			return result;
		}

		public static bool BringWindowToFrontByID(int pid){

			uint _pid = 0;
			monoflow.NativeMethods.EnumWindowsProc callback =  (IntPtr hWnd, IntPtr lParam)=> { 

				 monoflow.NativeMethods.GetWindowThreadProcessId(hWnd,out _pid);
				if(_pid == pid){
					monoflow.NativeMethods.SetForegroundWindow(hWnd); 
					return false;//stop callback call
				}
				return true;
			};

			monoflow.NativeMethods.EnumWindows(callback, IntPtr.Zero);

//NET easy solution, but mono always return 0 for MainWindowHandle :-(
//			try
//			{
//				IntPtr WindowHandle = IntPtr.Zero;
//				Process p = Process.GetProcessById(pid);
//				WindowHandle = p.MainWindowHandle;
//				monoflow.NativeMethods.SetForegroundWindow(WindowHandle); 
//
//			}
//			catch (Exception e) {return false;}

			return _pid != 0;
		}


		#endregion


		#region EditorMethods

		#if UNITY_EDITOR
		public static string GetScriptFolderFromScriptableObject(ScriptableObject so){
			//not tested!!!
			var script = MonoScript.FromScriptableObject(so);
			var scriptPath = AssetDatabase.GetAssetPath( script );
			var scriptFolder = Path.GetDirectoryName( scriptPath ) ;
			scriptFolder = scriptFolder.Replace("Assets/","");
			UnityEngine.Debug.Log("scriptPath: "+scriptPath);
			return scriptFolder;
		}

		#endif
		#endregion EditorMethods

		public static void Install(string path){
			string installPath = Path.Combine(Application.dataPath,path).Replace(@"\", "/");
			StartProcess(installPath,true);
		}

	


		/// <summary>
		/// Makes a texture specified by the parameters.
		/// </summary>
		/// <returns>The texture.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="col">Color.</param>
		public static Texture2D MakeTexture( int width, int height, Color col ) {
			Color[] pix = new Color[width * height];
			
			for( int i = 0; i < pix.Length; ++i ) {
				pix[ i ] = col;	
			}
			
			Texture2D result = new Texture2D( width, height );
			result.hideFlags = HideFlags.HideAndDontSave;//??
			result.SetPixels( pix );
			result.Apply();
			return result;
		}

		/// <summary>
		/// Draws the texture.
		/// </summary>
		/// <param name="tex">Tex.</param>
		public static void DrawTexture(Texture tex) {
			if(tex == null) return;
			Rect rect = GUILayoutUtility.GetRect(tex.width, tex.height);
			GUI.DrawTexture(rect, tex);
		}
		
		/// <summary>
		/// Draws a clickable texture.
		/// The event is triggerd on MouseUp
		/// </summary>
		/// <param name="tex">The texture you want to display</param>
		/// <param name="evt">Event that should be called when the user clicks on the texture</param>
		public static void DrawClickableTexture(Texture tex,Action evt ) {
			if(tex == null) return;
			Rect rect = GUILayoutUtility.GetRect(tex.width, tex.height);
			GUI.DrawTexture(rect, tex);
			
			var e = Event.current;
			if (e.type == EventType.MouseUp) {
				if (rect.Contains(e.mousePosition)) {
					if(evt != null) evt();
				}
			}
		}
		
		
		/// <summary>
		/// Draws a clickable texture horizontal.
		/// </summary>
		/// <param name="tex">The texture you want to display</param>
		/// <param name="evt">Event that should be called when the user clicks on the texture</param>
		public static void DrawClickableTextureHorizontal(Texture2D tex,Action evt){
			GUILayout.BeginHorizontal();
			DrawClickableTexture (tex,evt);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(2f);
		}
		
		
		public static bool IsMouseUpInArea (Rect area)
		{
			Event evt = Event.current;
			switch(evt.type){
			case EventType.MouseUp:
				if (area.Contains(evt.mousePosition)) {
					return true;
				}
				break;
			}
			return false;
		}



	}

}




