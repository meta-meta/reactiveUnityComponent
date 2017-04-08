/*
* UniOSC
* Copyright © 2014-2015 Stefan Schlupek
* All rights reserved
* info@monoflow.org
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine.Events;

namespace UniOSC{

	[CustomEditor(typeof(UniOSCUnityEventRelay),true)]
	[CanEditMultipleObjects]
	public class UniOSCUnityEventRelayEditor : UniOSCEventTargetEditor
    {
       
		//protected UniOSCEventTargetUnityEvent _target;
       
        protected SerializedProperty unioscEventProp;

        public override void OnEnable()
        {

            base.OnEnable();

            // if (target != _target) _target = target as UniOSCEventTargetUnityEvent;

            unioscEventProp = serializedObject.FindProperty("unioscEvent");

        }
        

		
		 public override void OnInspectorGUI(){


            serializedObject.Update();
          
            base.OnInspectorGUI();

            GUILayout.Space(10f);
           
            EditorGUILayout.PropertyField(unioscEventProp);
           
            serializedObject.ApplyModifiedProperties();
        }
        

	}
}