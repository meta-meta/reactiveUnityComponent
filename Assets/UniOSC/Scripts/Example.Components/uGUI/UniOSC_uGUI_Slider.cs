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
    [AddComponentMenu("UniOSC/EventDispatcherSlider_uGUI")]
    [RequireComponent(typeof(Slider))]
    public class UniOSC_uGUI_Slider : UniOSCEventDispatcher, IPointerUpHandler
    {

         protected Slider _slider;
        public override void Awake()
        {
            base.Awake();
            _slider = GetComponent<Slider>();

        }

        public override void OnEnable()
        {
            base.OnEnable();
            ClearData();
            AppendData(0f);
            if (_slider != null) _slider.onValueChanged.AddListener(SendOSCMessage);
           
            // _toggle.onValueChanged.AddListener()
        }

        public override void OnDisable()
        {
            if (_slider != null) _slider.onValueChanged.RemoveListener(SendOSCMessage);
            base.OnDisable();

        }


        public void SendOSCMessage(float data)
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




        public void OnPointerUp(PointerEventData eventData)
        {
           //For Security?
           // SendOSCMessage(_slider.value);
        }
    }
}
