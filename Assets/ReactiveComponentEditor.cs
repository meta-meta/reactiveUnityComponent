using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReactiveComponent))]
public class ReactiveComponentEditor : Editor
{
    private bool btn;
    private string gameObjectName = "";
    private GameObject gameObject;

    public override void OnInspectorGUI()
    {
        var component = target as ReactiveComponent;

        var gameObjStyle = new GUIStyle(GUI.skin.textField);
        gameObjStyle.fontStyle = gameObject == null ? FontStyle.Normal : FontStyle.Bold;
        gameObjectName = GUILayout.TextField(gameObjectName, gameObjStyle);
        gameObject = GameObject.Find(gameObjectName);

        GUI.enabled = gameObject != null;
        if (GUILayout.Button("add"))
        {
            component.gameObjects.Add(gameObject);
            gameObjectName = "";
        }
        GUI.enabled = true;

        component.gameObjects.ToList().ForEach(g =>
        {
            GUILayout.Label(g.name);
            var pos = g.transform.position;
            GUILayout.Label("Position: " + pos.x + " " + pos.y + " " + pos.z);
            if (GUILayout.Button("doubleThis"))
            {
                component.position = () => g.transform.position * 2;
            }

            var rot = g.transform.rotation.eulerAngles;
            GUILayout.Label("Rotation: " + rot.x + " " + rot.y + " " + rot.z);
            if (GUILayout.Button("tripleTHis"))
            {
                component.rotation = () => Quaternion.Euler(g.transform.rotation.eulerAngles * 3);
            }
        });

        component.PropertiesToFunctions.ToList().ForEach(pair =>
        {
            var prop = pair.Key;
            var propType = pair.Value;
            GUILayout.Label(prop.ToUpper() + " " + propType.ToString());

            // get the functions that have a return type matching this property's type
            component.Functions
                .Where((p) => p.Value.GetType().GetGenericArguments().Last() == propType)
                .ToList()
                .ForEach(p =>
                {
                    GUILayout.Label("fn: " + p.Key);
                    var allArgs = p.Value.GetType().GetGenericArguments();
                    allArgs.Take(allArgs.Length - 1).ToList().ForEach(arg =>
                    {
                        GUILayout.Label("--" + arg.FullName);
                        // TODO: this is where you dig through other GameObjects' properties for things that match this param's datatype

                        switch (arg.FullName)
                        {
                            case "System.Single":
                                var currProp = component.State.ContainsKey(prop) ? component.State[prop] as Dictionary<string, object> : new Dictionary<string, object>();
                                var param = p.Key;
                                var val = currProp.ContainsKey(param) ? currProp[param] : ""; // TODO: float
                                var nextVal = GUILayout.TextField((string)val); // TODO: OR dig through other GameObjects/components
                                GUILayout.Label("----" + val);
                                currProp[param] = nextVal;
                                component.State[prop] = currProp;
                                // TODO: convert to float
                                break;

                            case "UnityEngine.Vector3":
                                break;


                                // TODO: apply function to anything as adapter. construct chains of relationships node 
                                // [GameObject Component Param]--[fn]--[fn]--<list fns with matching types>--[ReactiveComponent prop]
                        }
                    });
                });
        });

    }
}