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

    private Dictionary<Type, Func<object, object>> TypesToControls = new Dictionary<Type, Func<object, object>>
    {
        {
            typeof (float), (val) => EditorGUILayout.FloatField(null == val ? new float() : (float)val) // TODO: OR dig through other GameObjects/components
        }

    };



    public override void OnInspectorGUI()
    {
        var component = target as ReactiveComponent;

        GameOptionsMenu(component);


//        component.gameObjects.ToList().ForEach(g =>
//        {
//            GUILayout.Label(g.name);
//            var pos = g.transform.position;
//            GUILayout.Label("Position: " + pos.x + " " + pos.y + " " + pos.z);
//            if (GUILayout.Button("doubleThis"))
//            {
//                component.position = () => g.transform.position * 2;
//            }
//
//            var rot = g.transform.rotation.eulerAngles;
//            GUILayout.Label("Rotation: " + rot.x + " " + rot.y + " " + rot.z);
//            if (GUILayout.Button("tripleTHis"))
//            {
//                component.rotation = () => Quaternion.Euler(g.transform.rotation.eulerAngles * 3);
//            }
//        });


        component.PropsToFunctionTypes.ToList().ForEach(pair =>
        {
            var prop = pair.Key;
            var propType = pair.Value;
            GUILayout.Label(prop.ToUpper() + " " + propType.ToString());
            
            // get the functions that have a return type matching this property's type
            var fns = component.Functions
                .Where(p => p.Value.GetType().GetGenericArguments().Last() == propType)
                .OrderBy(p => p.Key)
                .Select(p => p.Key);

            DropDown("--Assign-Function--", fns, selectedIndex =>
            {
                component.AssignFnToProp(prop, fns.ElementAt(selectedIndex));
            });

            if (component.PropsToFunctions.ContainsKey(prop))
            {
                var assignedFn = component.GetAssignedFn(prop);
                var fn = component.Functions[assignedFn];
                GUILayout.Label("fn: " + assignedFn);
                var genericArgs = fn.GetType().GetGenericArguments();
                var fnParams = genericArgs.Take(genericArgs.Length - 1); // the last arg is the delegate's return value

                fnParams.ToList().ForEachWithIndex((param, i) =>
                {
                    GUILayout.Label("--" + param.FullName);
                    // TODO: this is where you dig through other GameObjects' properties for things that match this param's datatype

                    if (TypesToControls.ContainsKey(param))
                    {
                        var args = component.GetArgs(prop);
                        var nextVal = TypesToControls[param].Invoke(args[i]);
                        component.SetArg(prop, i, nextVal);
                    }
                    else
                    {
                        // Debug.Log("Control not found for arg of type " + arg.FullName + " at position: " + i);
                    }

                    // TODO: apply function to anything as adapter. construct chains of relationships node 
                    // [GameObject Component Param]--[fn]--[fn]--<list fns with matching types>--[ReactiveComponent prop]
                });
            }
        });

    }

    private GameObject selectedGameObject;
    private void GameOptionsMenu(ReactiveComponent component)
    {
        var gameObjects = FindObjectsOfType<GameObject>()
            .Where(g => g.activeInHierarchy)
            .OrderBy(g => g.name);

        DropDown("--Game-Objects--", gameObjects.Select(g => g.name), selectedIndex =>
        {
            selectedGameObject = gameObjects.ElementAt(selectedIndex);
        });
    }

    private void DropDown(string title, IEnumerable<string> options, Action<int> onSelect)
    {
        var allOptions = options.Concat(new[] { title }).ToArray();
        var selectedIndex = EditorGUILayout.Popup(allOptions.Length - 1, allOptions);
        if (selectedIndex < allOptions.Length - 1)
        {
            onSelect.Invoke(selectedIndex);
        }
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