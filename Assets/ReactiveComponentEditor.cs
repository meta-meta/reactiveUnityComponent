using System;
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
            var propName = pair.Key;
            var type = pair.Value;
            GUILayout.Label(propName + " " + type.ToString());

            // get the functions that have a return type matching this property's type
            component.Functions
                .Where((p) => p.Value.GetType().GetGenericArguments().Last() == type)
                .ToList()
                .ForEach(p =>
                {
                    GUILayout.Label(p.Key);
                });
        });

    }
}