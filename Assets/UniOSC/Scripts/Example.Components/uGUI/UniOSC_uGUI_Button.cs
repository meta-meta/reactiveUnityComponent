/*
* UniOSC
* Copyright © 2014-2015 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using OSCsharp.Data;


namespace UniOSC{

	/// <summary>
	/// Dispatcher button that forces a OSCConnection to send a OSC Message.
	/// Two separate states: Down and Up 
	/// </summary>
   
	[AddComponentMenu("UniOSC/EventDispatcherButton_uGUI")]
	//[ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UniOSC_uGUI_Button : UniOSCEventDispatcher, IPointerDownHandler, IPointerUpHandler
    {  
        public enum ButtonMode { Down,Up,UpAndDown}
 
        public float OSCDataValueDown = 1;
        public float OSCDataValueUp = 1;

        public ButtonMode buttonMode= ButtonMode.Down;

        public override void Awake()
        {        
            base.Awake();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            ClearData();
            AppendData(0f);


        }
        public override void OnDisable()
        {
            base.OnDisable();
        }
       


        public void SendOSCMessage(object data)
        {
            //Debug.Log(GetType().Name+".SendOSCMessage");
            if (_OSCeArg.Packet is OscMessage)
            {
                ((OscMessage)_OSCeArg.Packet).UpdateDataAt(0, data);
            }
            else if (_OSCeArg.Packet is OscBundle)
            {
                foreach (OscMessage m in ((OscBundle)_OSCeArg.Packet).Messages)
                {
                    m.UpdateDataAt(0, data);
                }
            }


            _SendOSCMessage(_OSCeArg);
        }


        //IPointerDownHandler
       public void OnPointerDown(PointerEventData eventData)
        {
            if (buttonMode != ButtonMode.UpAndDown)
            {
                if (buttonMode != ButtonMode.Down) return;
            }
 
            SendOSCMessage(OSCDataValueDown);
        }

       //IPointerUpHandler
        public  void OnPointerUp(PointerEventData eventData)
        {
            if (buttonMode != ButtonMode.UpAndDown)
            {
                if (buttonMode != ButtonMode.Up) return;
            }
        
            SendOSCMessage(OSCDataValueUp);
        }
      
    }
}