/*
* JACK4U
* Copyright © 2014 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

[Serializable]
public class JACK4UConfigObj : ScriptableObject {

	public const string jackdPathDefault32 = @"C:/Program Files/Jack/jackd.exe";
	public const string qjackctlPathDefault64 = @"C:/Program Files (x86)/Jack/qjackctl.exe";
	public const string qjackctlPathDefault32 = @"C:/Program Files/Jack/qjackctl.exe";
	public const string abletonLivePathDefault = @"C:/ProgramData/Ableton/Live 9 Suite/Program/Ableton Live 9 Suite.exe";

	public const string JackRouterDllRegisterCommand =  @"regsvr32";
	public const string JackRouterDllPath64 = @"C:/Program Files (x86)/Jack/64bits/JackRouter.dll";

	public const string Asio4AllPath =@"JACK4U/External/ASIO4ALL_2_10_English.exe";
	public const string JackConnectionKitPath32 =@"JACK4U/External/Jack_v1.9.10_32_setup.exe";
	public const string JackConnectionKitPath64 =@"JACK4U/External/Jack_v1.9.10_64_setup.exe";

	
	public string QjackctlPath = String.Empty;
	public string DAWPath = String.Empty;

	public  int QjackctlProcessID;
	public  int DAWProcessID;

	[HideInInspector]
	public  int toolbarInt;


	public bool isInitialized{
		get{return _isInitialized;}
	}

	[SerializeField,HideInInspector]
	private bool _isInitialized;

	public void OnEnable() {

	}
}
