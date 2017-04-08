/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using JACK4U;
using System;


[InitializeOnLoad]
public class JACK4UAutoRun  {

	/// <summary>
	/// Is called when open the Unity editor, or after recompilation.
	/// Triggers some initialization routines so everthing is displayed correctly on startup.
	/// </summary>
	static JACK4UAutoRun(){

		if(!Application.HasProLicense()){
			return;
		}else{
			Application.runInBackground = true;
		}
	
		//check if we are run this process as admin
		JACK4UConnection.SetProcessElevation();

		//we don't need all API's as we only use ASIO on Windows but for future versions:
		//UniJackConnectionEditor.GetAvailableAPIs();

		//JACK4UEditor.Init();
		//JACK4UEditor.ShowEditor();
		//JACK4UEditor.CloseEditor();
	}
}
