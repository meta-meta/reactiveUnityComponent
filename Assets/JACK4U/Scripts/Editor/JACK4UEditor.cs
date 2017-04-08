/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace JACK4U{

	public class JACK4UEditor : EditorWindow {

		#region public
		public static string QjackctlPath{get{return _config.QjackctlPath;}}
		public static string DAWPath{get{return _config.DAWPath;}}
		public static JACK4UEditor Instance { get; private set; }
		public static GUIStyle style;
		#endregion

		#region private
		private static string[] _toolbarStrings = new string[] { "Installation","Settings","Controls"};

		[SerializeField]
		private static Texture2D _tex;
		[SerializeField]
		private static Texture2D _tex_logo;
		private static JACK4UConfigObj _config;
		private static EditorWindow _windowSelf;
		#endregion


		[MenuItem(JACK4UUtils.MENUITEM_EDITOR)]
		public static void ShowEditor(){
			_windowSelf = EditorWindow.GetWindow(typeof(JACK4UEditor));
			_windowSelf.minSize = new Vector2(300f,450f);
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6
        _windowSelf.title="JACK4U";
#else
            _windowSelf.titleContent = new GUIContent("JACK4U1", "JACK4U2");
#endif
           
			_windowSelf.autoRepaintOnSceneChange = true;
		}

		public static void CloseEditor(){
			if(_windowSelf == null)return;
			_windowSelf.Close();
		}


		public void OnEnable() {
			EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
			EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
			Instance = this;
			Init();
		}

		public static void SetDirtyEx(){
			#if UNITY_EDITOR
			EditorUtility.SetDirty(_config);
			#endif
		}

				
		public static void Init(){

			if(_tex == null)  _tex = JACK4UUtils.MakeTexture(2,2, new Color(0.95f,0.95f,0.95f,1.0f));
			if(_tex_logo == null) _tex_logo = Resources.Load(JACK4UUtils.JACK32_NAME,typeof(Texture2D)) as Texture2D;

			_ReadSettings();

			_config.QjackctlProcessID =JACK4UUtils.GetProcessIdByName( JACK4UUtils.QJACKCTL_NAME);
		}


		public void OnDisable() {
			EditorApplication.playmodeStateChanged -= PlaymodeStateChanged;
		}

		public void OnDestroy(){
			SetDirtyEx();
		}

		// called at 10 frames per second to give the inspector a chance to update.
		void OnInspectorUpdate(){
			//with unity 4 we loose Tooltips with this :-( 
			Repaint();
		}

		private void PlaymodeStateChanged()
		{

		}

		#region GUI

		public static void CreateStyle(){
			style = new GUIStyle(GUI.skin.box);
			style.normal.background =_tex;
			style.normal.textColor = Color.white;
			style.margin = new RectOffset(0,0,5,0);
			style.padding = new RectOffset(0,0,0,0);
		}

		void OnGUI(){

			GUILayout.Space(10);

			CreateStyle();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(GUILayout.Width(300));
			#region toolbar
			_config.toolbarInt =  GUILayout.Toolbar(_config.toolbarInt, _toolbarStrings,GUILayout.Width(100*_toolbarStrings.Count()),GUILayout.Height(30f));
			#endregion toolbar
			GUILayout.Space(10f);
			switch(_config.toolbarInt){
			case 0:
				OnGUI_Installation();
				break;
			case 1:
				OnGUI_Settings();
				break;
			case 2:
				OnGUI_Controls();
				break;
			}

			GUILayout.FlexibleSpace();
			#region logo
			if(_tex_logo != null){
				GUILayout.BeginVertical();

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
						JACK4UUtils.DrawTexture(_tex_logo);
					GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
				GUILayout.Space(20f);
			}
			#endregion logo


			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();


		}


		void OnGUI_Installation(){
			GUILayoutOption bh = GUILayout.Height(30f);
			if(GUILayout.Button(new GUIContent("Install ASIO4All driver","Installs the ASIO4ALL free ASIO driver."),bh)){
				JACK4UUtils.Install(JACK4UConfigObj.Asio4AllPath);
			}

			GUILayout.Space(10f);

			if(GUILayout.Button(new GUIContent("Install JACK Audio Connection Kit 4 Windows","Installs the JACK Audio Connection Kit 4 Windows.(qjackctl, jackd..)"),bh)){
				
				JACK4UUtils.Install(JACK4UUtils.is64BitOperatingSystem ? JACK4UConfigObj.JackConnectionKitPath64 : JACK4UConfigObj.JackConnectionKitPath32);
			}

			GUILayout.Space(10f);
			
			if(GUILayout.Button(new GUIContent("Register 64bit JackRouter.dll","You have to install the JACK Audio Connection Kit first in the default location!"),bh)){
				RegisterJackRouterDll(true);
			}

			GUILayout.Space(10f);

			if(GUILayout.Button(new GUIContent("UnRegister 64bit JackRouter.dll",""),bh)){
				RegisterJackRouterDll(false);
			}
		}


		void OnGUI_Settings(){
			GUILayoutOption bh = GUILayout.Height(30f);

			if(GUILayout.Button(new GUIContent("Set Path To qjackctl.exe",""),bh)){
				SetqjackctlPath();
			}
			DrawPath(QjackctlPath);

			GUILayout.Space(10);
		
			if(GUILayout.Button(new GUIContent("Set Path To DAW exe",""),bh)){
				SetDAWPath();
			}
			DrawPath(DAWPath);

			GUILayout.Space(10);
		}


		void OnGUI_Controls(){

			DrawConnection();
			GUILayout.Space(10);

			DrawRestartButtons();
		
			if(Application.isPlaying){
				GUILayout.Space(20);
				DrawStartJackAudioButton();
			}else{
				GUILayout.Space(60);
			}
			GUILayout.Space(20);

			GUILayout.BeginVertical("box");
			Draw_qjackct();
			Draw_DAW();
			GUILayout.EndVertical();

			GUILayout.BeginVertical("box");
			DrawTaskmgr();
			GUILayout.EndVertical();
			GUILayout.Space(20);

		}
		#endregion GUI


	

		#region Draw

		public static void DrawAdminStatus(){
			Rect area = GUILayoutUtility.GetRect (195.0f, 30.0f);
			area.width*=1f;
			
			if(JACK4UConnection.IsProcessElevated){
				EditorGUI.HelpBox(area,"Unity is running with administrator privileges", MessageType.Info);
			}else{
				EditorGUI.HelpBox(area,"Unity is running with normal user privileges", MessageType.Info);
			}
		}
		
		public static void DrawConnection(){
		
			if(JACK4UConnection.instance == null){
				if(GUILayout.Button(new GUIContent("Create JACK4U Connection","There have to be one JACK4U Connection component in your scene to connect to the JACK Audio system."),GUILayout.Height(30) )){
					CreateJackConnection();
				}
			}else{
				JACK4UConnection pc = JACK4UConnection.instance;

				if(GUILayout.Button(new GUIContent("Select JACK4U Connection in Hierachy",""),GUILayout.Height(30)) ){
					EditorGUIUtility.PingObject(pc);
					Selection.activeGameObject = pc.gameObject;
				}
			}		
			
		}
		#endregion Draw

		#region Restart


		public static void DrawRestartButtons(){

			GUILayout.BeginVertical("box");


			if(JACK4UConnection.IsUacEnabled){
				DrawAdminStatus();
			}


			GUIContent gc = new GUIContent();
			
			
			//if(UniJackConnection.IsUacEnabled){
			if(JACK4UConnection.IsProcessElevated){
				//only one Button as we can't start a process with lesser privileges than the running.
//				gc.text = "Restart Unity with administrator privilege";
//				gc.tooltip ="If you have problems with connecting to the JackAudio system you should restart Unity again with administrator privilege";
//				DrawRestartButton(gc,true);
			}else{
				//problem with deactivated UAC. Mono always think IsProcessElevated is false,so beware if you restart as user (will be as admin)
				gc.text = "Restart Unity with administrator privilege";
				gc.tooltip ="If you have turned on UAC on Windows you will be prompt by the operation system. Confirm that you allow this action and Unity will restart with admin privilege.";
				DrawRestartButton(gc,true);
//				gc.text = "Restart Unity with normal user privilege";
//				gc.tooltip ="Restart Unity with user privilege.This doesn't work if you have already admin privileges!";
//				DrawRestartButton(gc,false);
			}
			//			}else{
			//				//UAC is off
			//				gc.text = "Restart Unity";
			//				gc.tooltip ="Restart Unity";
			//				DrawRestartButton(gc,false);
			//			}


			GUILayout.EndVertical();
			
		}
		
		private static void DrawRestartButton(GUIContent gc, bool asAdmin){
			if( GUILayout.Button(gc,GUILayout.Height(30f))){
				
				if( EditorApplication.SaveCurrentSceneIfUserWantsTo()){
					JACK4UEditor.SetDirtyEx();
					//we need to give Unity some time so we start a new Thread that has some delay before we restart.
					Thread thread = new Thread(new ParameterizedThreadStart(WorkThreadFunction));
					thread.Start(asAdmin);
				}
			}
		}

		private static void WorkThreadFunction(object asAdmin){
			Thread.Sleep(500);
			JACK4UUtils.RestartCurrentProcess((bool)asAdmin);
		}
		#endregion

		#region drawButtons
		

		
		public static void DrawStartJackAudioButton ()
		{
			GUILayout.BeginHorizontal("box");

			GUIContent gc = new GUIContent("","");
			gc.text = JACK4UAudio.isRunning ? "Disconnect from JACK Audio" : "Connect to JACK Audio";
			if( GUILayout.Button(gc,GUILayout.Height(30f))){

				if(JACK4UAudio.isRunning){
					JACK4UConnection.instance.StopPortAudio();
				}else{ 
					JACK4UConnection.instance.StartPortAudio();
				}

			}
			DrawPortAudioStatus(25f,25f);
			GUILayout.EndHorizontal();
		}

		public static void DrawPortAudioStatus(float w,float h){
			if(style == null) return;

			if(JACK4UAudio.isRunning){
				GUI.backgroundColor = Color.green;
			}else{
				GUI.backgroundColor = Color.red;
			}

			GUILayout.Box("",style,GUILayout.Width(w),GUILayout.Height(h));
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
		
		}
		
		public static void Draw_qjackct(){


			if(String.IsNullOrEmpty(QjackctlPath)){
				Rect area = GUILayoutUtility.GetRect (195.0f, 40.0f);
				area.width*=1f;
				EditorGUI.HelpBox(area,"Please specify a path to the JACK Audio Control application (qjackctl.exe)", MessageType.Info);
				return;
			}

			GUILayout.BeginHorizontal();

			if(JACK4UEditor.IsProcessRunning(App.Qjackctl)){

				if(GUILayout.Button(new GUIContent("Close Qjackctl",""),GUILayout.Height(30))){
					KillProcess(App.Qjackctl);
				}
				
			}else{

				if(GUILayout.Button(new GUIContent("Open Qjackctl",""),GUILayout.Height(30))){

					Process p = JACK4UUtils.StartProcess(QjackctlPath,JACK4UConnection.IsProcessElevated);
					if(p != null) {
						p.Exited+= (object sender, EventArgs e)=>{SetQjackctlProcessID(0);};
						SetQjackctlProcessID(p.Id);
						SetDirtyEx();
					}
				}
			}

			Rect area2 = GUILayoutUtility.GetRect(25f,25,style);
			DrawProcessStatus(area2,IsProcessRunning(App.Qjackctl));

			if(JACK4UUtils.IsMouseUpInArea(area2)){
				JACK4UUtils.BringWindowToFrontByID(_config.QjackctlProcessID);
			}

			GUILayout.EndHorizontal();
			
		}
		
		
		public static void Draw_DAW(){
			
			if(String.IsNullOrEmpty(DAWPath)){
				Rect area = GUILayoutUtility.GetRect (195.0f, 40.0f);
				area.width*=1f;
				EditorGUI.HelpBox(area,"Please specify a path to the DAW application (yourDAW.exe)", MessageType.Info);
				return;
			}
			
			GUILayout.BeginHorizontal();

			if(JACK4UEditor.IsProcessRunning(App.DAW)){

				if(GUILayout.Button(new GUIContent("Close DAW",""),GUILayout.Height(30))){
					KillProcess(App.DAW);
				}
				
			}else{

				if(GUILayout.Button(new GUIContent("Open DAW",""),GUILayout.Height(30))){
					Process p = JACK4UUtils.StartProcess(DAWPath,JACK4UConnection.IsProcessElevated);
					if(p != null){
						p.Exited+= (object sender, EventArgs e)=>{SetDAWProcessID(0);};
						SetDAWProcessID( p.Id);
						SetDirtyEx();
					}
				}
				
			}

			Rect area2 = GUILayoutUtility.GetRect(25f,25,style);
			DrawProcessStatus(area2,IsProcessRunning(App.DAW));

			if(JACK4UUtils.IsMouseUpInArea(area2)){
				JACK4UUtils.BringWindowToFrontByID(_config.DAWProcessID);
			}

			GUILayout.EndHorizontal();
			
		}

		public static void DrawTaskmgr(){

			GUILayout.BeginHorizontal();
			if(GUILayout.Button(new GUIContent("Start Task Manager",""),GUILayout.Height(30))){

				Process p = JACK4UUtils.StartProcess("taskmgr.exe",JACK4UConnection.IsProcessElevated);
				if(p != null) {
					p.Exited+= (object sender, EventArgs e)=>{};

					SetDirtyEx();
				}
				
			}

			GUILayout.EndHorizontal();
		}


		#endregion


		private void DrawPath(string path){

			if(String.IsNullOrEmpty(path)){
				GUI.backgroundColor = Color.red;
				GUILayout.Label(new GUIContent("No path is specified!",""),style,GUILayout.ExpandWidth(true));
			}else{
				GUI.backgroundColor = Color.clear;
				GUIContent guic = new GUIContent(path,"");
				Rect area = GUILayoutUtility.GetRect(guic, style,GUILayout.ExpandWidth(true));
				GUI.Label(area,guic,style);
				if(JACK4UUtils.IsMouseUpInArea(area)){
					string itemPath = path.Replace(@"/", @"\");  
					System.Diagnostics.Process.Start("explorer.exe", "/select,"+itemPath);
				}

			}
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
		}

		private static void DrawProcessStatus(Rect area, bool isRunning){

			GUI.backgroundColor =isRunning ? Color.green : Color.red;
			GUI.Box(area,"",style);
			GUI.backgroundColor = Color.white;
			GUI.contentColor = Color.white;
		}


		[MenuItem(JACK4UUtils.MENUITEM_CREATE_CONNECTION,false,1)]
		static void CreateJackConnection(){

			_CreateJackConnection();
		}
		
		static void _CreateJackConnection(){

			JACK4UConnection jc = FindObjectOfType(typeof(JACK4UConnection)) as JACK4UConnection;
			if(jc == null){
				GameObject go = new GameObject("JACK4U Connection");
				//go.AddComponent<AudioListener>();//optional? Not in Unity 5!!! Don't use a source and a listener on the same gameobject
				go.AddComponent<JACK4UConnection>();
			}else{
				#if UNITY_EDITOR
				EditorUtility.DisplayDialog("No JACK4U Connection could be created","There is already a JACK4U Connection component in your scene! Only one is allowed","OK");
				#endif
			}
		}

		#region Paths

		//[MenuItem(JACK4UUtils.MENUITEM_SET_QJACKCTL_PATH)]
		public static string SetqjackctlPath(){

			if(_config == null)return null;
			if(String.IsNullOrEmpty(_config.QjackctlPath ))_config.QjackctlPath = JACK4UUtils.is64BitOperatingSystem ?  JACK4UConfigObj.qjackctlPathDefault64 : JACK4UConfigObj.qjackctlPathDefault32;
			
			var tempPath = 	EditorUtility.OpenFilePanel("Path to qjackctl.exe",Path.GetDirectoryName(_config.QjackctlPath),"exe");
			if(!String.IsNullOrEmpty(tempPath)) _config.QjackctlPath = tempPath;
			SetDirtyEx();
			return _config.QjackctlPath;
		
		}
		
		//[MenuItem(JACK4UUtils.MENUITEM_SET_DAW_PATH)]
		public static string SetDAWPath(){

			if(_config == null)return null;
			if(String.IsNullOrEmpty(_config.DAWPath ))_config.DAWPath =  JACK4UConfigObj.abletonLivePathDefault;
			
			var tempPath = 	EditorUtility.OpenFilePanel("Path to your DAW exe",Path.GetDirectoryName(_config.DAWPath),"exe");
			if(!String.IsNullOrEmpty(tempPath)) _config.DAWPath = tempPath;
			SetDirtyEx();
			return _config.DAWPath;
		}

		#endregion Paths


		#region Process

		public static void SetQjackctlProcessID(int val){		
			_config.QjackctlProcessID = val;
		}

		public static void SetDAWProcessID(int val){
			_config.DAWProcessID = val;
		}
		
		public static void RegisterJackRouterDll(bool register){
			
			ProcessStartInfo si = new ProcessStartInfo();
			si.FileName = JACK4UConfigObj.JackRouterDllRegisterCommand;
			
			if(register){
				si.Arguments="\""+JACK4UConfigObj.JackRouterDllPath64+"\"";
			}else{
				si.Arguments="-u "+"\""+JACK4UConfigObj.JackRouterDllPath64+"\"";
			}
			
			si.Verb ="runas";

			try{
				Process p= Process.Start(si);
				p.EnableRaisingEvents = true;	
			}catch{
				//return null;
			}
				
		}

		public enum App{
			DAW,
			Qjackctl
		}

		public static bool IsProcessRunning(App app){

			int id = 0;
			switch(app){
			case App.DAW :
				id= _config.DAWProcessID;
				break;
			case App.Qjackctl:
				id= _config.QjackctlProcessID;
				break;
			}
			if(id == 0)return false;
			try
			{
				Process.GetProcessById(id);
			}
			catch (ArgumentException )
			{
				return false;
			}
			
			return true;
		}

		public static bool KillProcess(App app){
			int id = 0;
			switch(app){
			case App.DAW :
				id= _config.DAWProcessID;
				break;
			case App.Qjackctl:
				id= _config.QjackctlProcessID;
				//when we close qjackctl we should close also the DAW. As the jackd process isn't killed this is not necessary.
//				if(IsProcessRunning(App.DAW)){
//					try
//					{
//						Process.GetProcessById(_config.DAWProcessID).CloseMainWindow();
//						_config.DAWProcessID = 0;
//					}
//					catch (ArgumentException ){}
//				}
				break;
			}
			
			if(id == 0)return false;
			
			try
			{
				Process p = Process.GetProcessById(id);
				p.CloseMainWindow();
				p.Close();
	
				switch(app){
				case App.DAW :
					_config.DAWProcessID=0;
					break;
				case App.Qjackctl:
					 _config.QjackctlProcessID=0;
					//could not get id from jackd 64 bit
					//int xid = UniJackUtils.GetProcessIdByName("jackd");

					break;
				}
				
			}
			catch (Exception )
			{
				return false;
			}
			
			return true;
			
		}
		#endregion


		public static void CreateSettings(){
			_ReadSettings();
		}
		
		private  static void _ReadSettings(){
			// file ending must be '.asset' because otherwise unity can't load the right type of the scriptableObject
			//this solution is portable: get path from current script and construct the path from there

			//this doesn't work when we call the Method trough [InitializeOnLoad] when we moved the project to a new location as the old stack is cached!
			//only workaround is to reimport the script.
			//string scriptPath = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();

			var script = MonoScript.FromScriptableObject( Instance );//only works when Editor window is visible .
			var scriptPath = AssetDatabase.GetAssetPath( script );

			var scriptFolder = Path.GetDirectoryName( scriptPath ) ;
			scriptFolder = scriptFolder.Replace("Assets/","");
			//UnityEngine.Debug.Log("scriptPath: "+scriptPath);
			
			string filepath =Path.Combine( Path.Combine(Application.dataPath,scriptFolder),JACK4UUtils.CONFIGPATH_EDITOR).Replace(@"\", "/"); 
			string relativFilepath = filepath.Replace(Application.dataPath, "Assets");
			//UnityEngine.Debug.Log("filepath: "+filepath);
			
			_config = AssetDatabase.LoadAssetAtPath(relativFilepath,typeof(JACK4UConfigObj) ) as JACK4UConfigObj;
			if(_config == null){
				//UnityEngine.Debug.Log("Try to create Configuration File at :"+relativFilepath);
				_config = ScriptableObject.CreateInstance<JACK4UConfigObj>();
				string directoryName =  Path.GetDirectoryName(filepath);
				try{
					if(!Directory.Exists(directoryName)){
						Directory.CreateDirectory(directoryName);
					}
					_config.name = relativFilepath;
					AssetDatabase.CreateAsset(_config, relativFilepath);
					AssetDatabase.SaveAssets();
					AssetDatabase.Refresh();
					//UnityEngine.Debug.Log("Configuration File was created at :"+relativFilepath);
				}
				
				catch(Exception e){
					UnityEngine.Debug.LogError("Generating the directory: "+directoryName+" failed!\n"+e.ToString());
				}
				
			}else{
				//UnityEngine.Debug.Log("Config is there and Loaded!");
			}
			
			
			if(_config == null){
				UnityEngine.Debug.LogError("configuration file couldn't loaded or created!");
				return;
			}
			
			
			
		}


	}
}
