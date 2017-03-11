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

    private Dictionary<Type, Func<object, object>> Controls = new Dictionary<Type, Func<object, object>>
    {
        {
            typeof (float), (val) =>
            {
                var text = GUILayout.TextField(val.ToString()); // TODO: OR dig through other GameObjects/components
                GUILayout.Label("---- " + val);
                return float.Parse(text); // TODO: tryparse
            }
        }

    };

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
                    allArgs.Take(allArgs.Length - 1).ToList().ForEachWithIndex((arg, i) =>
                    {
                        GUILayout.Label("--" + arg.FullName);
                        // TODO: this is where you dig through other GameObjects' properties for things that match this param's datatype

                        if (Controls.ContainsKey(arg))
                        {
                            var currParams = component.GetParams(prop);
                            var param = p.Key;
                            var val = currParams.ContainsKey(param) ? (float)currParams[param] : new float(); // TODO: float
                            var nextVal = Controls[arg].Invoke(val);
                            component.SetParam(prop, param, nextVal);
                        }
                        else
                        {
//                            Debug.Log("Control not found for arg of type " + arg.FullName + " at position: " + i);
                        }

                        // TODO: apply function to anything as adapter. construct chains of relationships node 
                        // [GameObject Component Param]--[fn]--[fn]--<list fns with matching types>--[ReactiveComponent prop]
                    });
                });
        });

    }
}

public static class ForEachExtensions
{
    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        int idx = 0;
        foreach (T item in enumerable)
            handler(item, idx++);
    }
}