/*
* UniOSC
* Copyright © 2014-2015 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OSCsharp.Data;

namespace UniOSC
{

    /// <summary>
    /// Demo class to show how you can use the UnityEvent system.
    /// Message is sent from a <see cref="UniOSCUnityEventRelay"/> instance  <![CDATA[UnityEvent<OscMessage>]]>
    /// </summary>
    public class UniOSCUnityEvent_Receiver : MonoBehaviour
    {
        public bool debug;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// This is a method you can add as listener to an UnityEvent&lt;OscMessage&gt;
        /// </summary>
        /// <param name="msg"></param>
        public void OnOSCMessageReceived(OscMessage msg)
        {
            if(debug) Debug.Log(string.Format("OnOSCMessageReceived: {0} :: data count:{1}", msg.Address,msg.Data.Count));

            //msg.Data  (get the data from the OSCMessage as an object[] array)

            //It is a good practice to always check the data count before accessing the data.
            // if(msg.Data.Count <1)return;

            /*to  check the data type we have several option:
            * a) 
            * if(  msg.Data[0] is System.Single)
            * 
            * b) 
            * if( msg.Data[0].GetType() == typeof(System.Single) )
            * 
            * c) check the typeTag (see OSC documentation : http://opensoundcontrol.org/spec-1_0 
            * typeTag is a string like ',f' for a single float or ',ff' for two floats
            * if( msg.TypeTag.Substring(1,1) == "f")
            */

            //Debug.Log("typeTag: "+((OscMessage)args.Packet).TypeTag);
            if (debug) { 
            for(int i = 0;i< msg.Data.Count; i++)
            {
                Debug.Log(msg.Data[i]);
            }
            Debug.Log("---------");
            }

        }

    }
}
