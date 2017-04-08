/*
* UniOSC
* Copyright © 2014-2016 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using OSCsharp.Data;

namespace UniOSC
{
    [AddComponentMenu("UniOSC/EventDispatcherToggle_uGUI")]
    [RequireComponent(typeof(Toggle))]
    public class UniOSC_uGUI_Toggle : UniOSCEventDispatcher
    {
        public float OSCDataValueON = 1;
        public float OSCDataValueOFF = 0;

        protected Toggle _toggle;

        public override void Awake()
        {
            base.Awake();
            _toggle = GetComponent<Toggle>();
           
        }

        public override void OnEnable()
        {
            base.OnEnable();
             ClearData();
            AppendData(0f);
           if(_toggle != null) _toggle.onValueChanged.AddListener(_SendOscData);
        }

        public override void OnDisable()
        {
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(_SendOscData);
            base.OnDisable();

        }

        private void _SendOscData(bool val)
        {
            if(val){
                SendOSCMessage(OSCDataValueON);
            }else{
                SendOSCMessage(OSCDataValueOFF);
            }
        }



        public void SendOSCMessage(object data)
        {
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

       
    }
}
