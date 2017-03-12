using System;
using System.Collections.Generic;
using UnityEngine;
using UnityScript.Lang;

public class ReactiveComponent : MonoBehaviour
{
    private static Func<Vector3, float, Func<Vector3>> multiplyVectorScalar = (Vector3 v, float f) => () => v * f;
    private static Func<Vector3, Vector3, Func<Vector3>> addVectors = (Vector3 v1, Vector3 v2) => () => v1 + v2;
    private static Func<Quaternion, float, Func<Quaternion>> multiplyRotation = (Quaternion q, float f) => () => Quaternion.Euler(q.eulerAngles * f);

    public Dictionary<string, Delegate> Functions = new Dictionary<string, Delegate>
    {
        {"multiplyVectorScalar", multiplyVectorScalar},
        {"addVectors", addVectors},
        {"multiplyRotation", multiplyRotation}
    };

    public Dictionary<string, Type> PropsToFunctionTypes = new Dictionary<string, Type>
    {
        {"localPosition", typeof(Func<Vector3>)},
        {"localRotation", typeof(Func<Quaternion>)}
    };

/* Property Names => Functions/Params
 * { "position": { "fn": "addVectors", "args": [1, 2.4, ...] } } */
    public Dictionary<string, object> PropsToFunctions = new Dictionary<string, object>();

    public void AssignFnToProp(string prop, string fn)
    {
        if (!PropsToFunctions.ContainsKey(prop))
        {
            PropsToFunctions[prop] = new Dictionary<string, object>();
        }

        var nextProp = PropsToFunctions.Get<Dictionary<string, object>>(prop);

        if (!nextProp.ContainsKey("fn") || !fn.Equals(nextProp["fn"]))
        {
            var numArgs = Functions[fn].GetType().GetGenericArguments().Length;
            Debug.Log("assigning " + fn + "(<" + numArgs + ">) to " + prop);
            nextProp["fn"] = fn;
            nextProp["args"] = new object[numArgs];
        }
    }

    private Dictionary<string, object> GetFnAssignment(string prop)
    {
        return PropsToFunctions.Get<Dictionary<string, object>>(prop);
    } 

    public string GetAssignedFn(string prop)
    {
        return GetFnAssignment(prop).Get<string>("fn");
    }

    public object[] GetArgs(string prop)
    {
        return GetFnAssignment(prop).Get<object[]>("args");
    }

    public void SetArg(string prop, int paramIndex, object val)
    {
        GetArgs(prop)[paramIndex] = val;
    }

    


    public HashSet<GameObject> gameObjects = new HashSet<GameObject>(); 

    public Dictionary<string, Func<Vector3, Vector3>> transformations = new Dictionary<string, Func<Vector3, Vector3>>();

    public Func<Vector3> position;
    public Func<Quaternion> rotation;

    // Use this for initialization
    void Start ()
    {

    }
	
	// Update is called once per frame
	void Update () {
        if (null != position)
        {
            transform.localPosition = position.Invoke();
        }
        if (null != rotation)
        {
            transform.localRotation = rotation.Invoke();
        }
    }


}

public static class DictionaryExcetions
{
    public static T Get<T>(this Dictionary<string, object> instance, string name)
    {
        return (T)instance[name];
    }

}