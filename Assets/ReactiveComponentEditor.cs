using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

[CustomEditor(typeof(ReactiveComponent))]
public class ReactiveComponentEditor : Editor
{
    private bool btn;
    private string gameObjectName = "";
    private GameObject gameObject;

    private Dictionary<Type, Func<Ref, Ref>> TypesToControls;

    public ReactiveComponentEditor()
    {
        TypesToControls = new Dictionary<Type, Func<Ref, Ref>>
        {
            {
                typeof (float), (val) =>
                {
                    var nextVal = EditorGUILayout.FloatField(null == val ? new float() : (float) val.Get());
                    return new Ref(() => nextVal);
                }
                // TODO: OR dig through other GameObjects/components
            }
        };

        TypesToControls.Add(typeof (Vector3), Vector3Control);
    }

    private Ref Vector3Control(Ref val)
    {
        var go = EditorGUILayout.ObjectField(
            selectedGameObject ?? new UnityEngine.Object(),
            typeof (GameObject),
            true);

        if (go.GetType() == typeof (GameObject))
        {
            selectedGameObject = (GameObject) go;
        }

        if (null != selectedGameObject)
        {
            var t = selectedGameObject.transform;

            var options = new Dictionary<string, Ref>
            {
                {"position", new Ref(() => t.position)},
                {"localPosition", new Ref(() => t.localPosition)},
                {"rotation.eulerAngles", new Ref(() => t.rotation.eulerAngles)},
                {"localEulerAngles", new Ref(() => t.localEulerAngles)},
                {"localScale", new Ref(() => t.localScale)},
                {"lossyScale", new Ref(() => t.lossyScale)},
            }.ToList();

            var selectedIndex = DropDown("--Select-Field--", options.Select(pair => pair.Key));
            if (selectedIndex < options.Count)
            {
                return options.ElementAt(selectedIndex).Value; // TODO: might need to be a fn to get current value
            }
        }

        return val;
    }



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
                var fnParamTypes = genericArgs.Take(genericArgs.Length - 1); // the last arg is the delegate's return value

                fnParamTypes.ToList().ForEachWithIndex((paramType, i) =>
                {
                    GUILayout.Label("param: " + paramType.FullName);
                    GUILayout.Label("arg: " + component.GetArgs(prop)[i]);
                    // TODO: this is where you dig through other GameObjects' properties for things that match this param's datatype

                    if (TypesToControls.ContainsKey(paramType))
                    {
                        var args = component.GetArgs(prop);
                        var nextVal = TypesToControls[paramType].Invoke(args[i]);
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

    private int DropDown(string title, IEnumerable<string> options)
    {
        var allOptions = options.Concat(new[] { title }).ToArray();
        return EditorGUILayout.Popup(allOptions.Length - 1, allOptions);
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

// http://stackoverflow.com/questions/24329012/store-reference-to-an-object-in-dictionary
public sealed class Ref
{
    public Func<object> Get { get; private set; }
    public Ref(Func<object> getter)
    {
        Get = getter;
    }

    public override string ToString()
    {
        return Get().ToString();
    }
}