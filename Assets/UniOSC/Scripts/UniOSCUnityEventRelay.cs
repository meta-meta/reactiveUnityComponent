/*
* UniOSC
* Copyright © 2014-2015 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using OSCsharp.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UniOSC
{
    [AddComponentMenu("UniOSC/UnityEventRelay")]
    public class UniOSCUnityEventRelay : UniOSCEventTarget
    {

        [System.Serializable]
        public class UnityEvent_OscMessage : UnityEvent<OscMessage>
        {
        }


        [HideInInspector]
        public UnityEvent_OscMessage unioscEvent;

        public override void Start()
        {
            //Don't forget this!!!!
            base.Start();
            //here your custom code
        }

        public override void OnEnable()
        {
            //optional 
           
            _Init();
            //Don't forget this!!!!
            base.OnEnable();
            //here your custom code
        }

        private void _Init()
        { }

        public override void OnDisable()
        {
            //Don't forget this!!!!
            base.OnDisable();
            //here your custom code
        }


        public override void Update()
        {
            //Don't forget this!!!!
            base.Update();
            //here your custom code
        }

        /// <summary>      
        /// Method is called from a OSCConnection when a OSC message arrives. 
        /// The OSCMessage is then relayed as a normal UnityEvent(OscMessage)
        /// </summary>
        /// <param name="args">OSCEventArgs</param>
        public override void OnOSCMessageReceived(UniOSCEventArgs args)
        {
           
            unioscEvent.Invoke(((OscMessage)args.Packet));

        }

    }
}
