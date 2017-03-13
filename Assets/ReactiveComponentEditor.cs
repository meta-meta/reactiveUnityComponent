using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ReactiveComponent))]
public class ReactiveComponentEditor : Editor
{
    private GameObject _selectedGameObject;
    private readonly Dictionary<Type, Func<Ref, Ref>> _typesToControls;

    public ReactiveComponentEditor()
    {
        _typesToControls = new Dictionary<Type, Func<Ref, Ref>>
        {
            {
                typeof (float), (val) =>
                {
                    var nextVal = EditorGUILayout.FloatField(null == val ? new float() : (float) val.Get());
                    return new Ref(() => nextVal); // TODO: OR dig through other GameObjects/components
                }
            },
            {typeof (Vector3), Vector3Control},
            {typeof (Quaternion), QuaternionControl}
        };
    }

    private Ref Vector3Control(Ref val)
    {
        GameObjectSelector(); // assigns _selectedGameObject

        var options = new Dictionary<string, Ref>
        {
            {"zero", new Ref(() => Vector3.zero)},
            {"one", new Ref(() => Vector3.one)},
            {"left", new Ref(() => Vector3.left)},
            {"right", new Ref(() => Vector3.right)},
            {"up", new Ref(() => Vector3.up)},
            {"down", new Ref(() => Vector3.down)},
            {"forward", new Ref(() => Vector3.forward)},
            {"back", new Ref(() => Vector3.back)},

        }.ToList();

        if (null != _selectedGameObject)
        {
            var t = _selectedGameObject.transform;

            var goOptions = new Dictionary<string, Ref>
            {
                {"position", new Ref(() => t.position)},
                {"localPosition", new Ref(() => t.localPosition)},
                {"rotation.eulerAngles", new Ref(() => t.rotation.eulerAngles)},
                {"localEulerAngles", new Ref(() => t.localEulerAngles)},
                {"localScale", new Ref(() => t.localScale)},
                {"lossyScale", new Ref(() => t.lossyScale)},
            }.ToList();

            options = options.Concat(goOptions).ToList();
        }

        var selectedIndex = DropDown("--Select-Field--", options.Select(pair => pair.Key));
        return selectedIndex < options.Count ? options.ElementAt(selectedIndex).Value : val;
    }

    private Ref QuaternionControl(Ref val)
    {
        GameObjectSelector(); // assigns _selectedGameObject

        var options = new Dictionary<string, Ref>
        {
            {"identity", new Ref(() => Quaternion.identity)},
        }.ToList();

        if (null != _selectedGameObject)
        {
            var t = _selectedGameObject.transform;

            var goOptions = new Dictionary<string, Ref>
            {
                {"identity", new Ref(() => Quaternion.identity)},
                {"rotation", new Ref(() => t.rotation)},
                {"localRotation", new Ref(() => t.localRotation)},
                // TODO: other functions ex: Vector3 -> Quaternion
            }.ToList();

            options = options.Concat(goOptions).ToList();
        }

        var selectedIndex = DropDown("--Select-Field--", options.Select(pair => pair.Key));
        return selectedIndex < options.Count ? options.ElementAt(selectedIndex).Value : val;
    }

    private void GameObjectSelector()
    {
        var go = EditorGUILayout.ObjectField(
            _selectedGameObject ?? new UnityEngine.Object(),
            typeof(GameObject),
            true);

        if (go is GameObject)
        {
            _selectedGameObject = (GameObject)go;
        }
    }

    public override void OnInspectorGUI()
    {
        var component = target as ReactiveComponent;

        component.PropsToTypes.ToList().ForEach(pair =>
        {
            var prop = pair.Key;
            var propType = pair.Value;
            GUILayout.Label(prop.ToUpper() + " " + propType.ToString());
            
            // get the functions that have a return type matching this property's type
            var fns = component.NamesToFunctions
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
                var fn = component.NamesToFunctions[assignedFn];
                GUILayout.Label("fn: " + assignedFn);
                var genericArgs = fn.GetType().GetGenericArguments();
                var fnParamTypes = genericArgs.Take(genericArgs.Length - 1); // the last arg is the delegate's return value

                fnParamTypes.ToList().ForEachWithIndex((paramType, i) =>
                {
                    GUILayout.Label("param: " + paramType.FullName);
                    GUILayout.Label("arg: " + component.GetArgs(prop)[i]);
                    // TODO: this is where you dig through other GameObjects' properties for things that match this param's datatype

                    if (_typesToControls.ContainsKey(paramType))
                    {
                        var args = component.GetArgs(prop);
                        var nextVal = _typesToControls[paramType].Invoke(args[i]);
                        component.SetArg(prop, i, nextVal);
                    }
                    else
                    {
                        GUILayout.Label("Control not found for arg of type " + paramType.FullName);
                    }

                    // TODO: apply function to anything as adapter. construct chains of relationships node 
                    // [GameObject Component Param]--[fn]--[fn]--<list fns with matching types>--[ReactiveComponent prop]
                });
            }
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