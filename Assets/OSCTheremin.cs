using System.Collections;
using System.Collections.Generic;
using OSCsharp.Data;
using UniOSC;
using UnityEngine;

public class OSCTheremin : UniOSCEventDispatcher
{
    private GameObject amplitudeCube;
    private GameObject frequencyCube;
    private GameObject controllerLeft;
    private GameObject controllerRight;
	// Use this for initialization
	void Start ()
	{
        amplitudeCube = GameObject.Find("AmplitudeCube");
        frequencyCube = GameObject.Find("FrequencyCube");
        controllerLeft = GameObject.Find("controller_left");
        controllerRight = GameObject.Find("controller_right");
	}

    float dist(GameObject go1, GameObject go2)
    {
        return Vector3.Distance(go1.transform.position, go2.transform.position);
    }

    void sendOSC(string address, object data)
    {
        ClearData();
        AppendData(address);
        AppendData(data);
        _SendOSCMessage(_OSCeArg);
    }

    // Update is called once per frame
    void Update ()
    {
        sendOSC("/freq", dist(frequencyCube, controllerRight));
        sendOSC("/amp", dist(amplitudeCube, controllerLeft));

        var priThumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        var secThumb = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        sendOSC("/pri/x", priThumb.x);
        sendOSC("/pri/y", priThumb.y);
        sendOSC("/sec/x", secThumb.x);
        sendOSC("/sec/y", secThumb.y);
    }
}
