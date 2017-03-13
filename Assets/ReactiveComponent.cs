using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityScript.Lang;

public class ReactiveComponent : MonoBehaviour
{
    // Vector3
    private static readonly Func<Vector3, float, Vector3> MultiplyVectorScalar = (v, f) => v * f;
    private static readonly Func<Vector3, Vector3, Vector3> AddVectors = (v1, v2) => v1 + v2;

    // Quaternion
    private static readonly Func<Quaternion, float, Quaternion> MultiplyEulerScalar = (q, f) => Quaternion.Euler(q.eulerAngles * f);
    private static readonly Func<Quaternion, Quaternion, Quaternion> DiffQuaternion = (q1, q2) => Quaternion.Inverse(q1) * q2;
    private static readonly Func<Quaternion, Quaternion, float, Quaternion> SlerpUnclampedQuaternion = (q1, q2, f) => Quaternion.SlerpUnclamped(q1, q2, f);

    public Dictionary<string, Delegate> NamesToFunctions = new Dictionary<string, Delegate>
    {
        {"MultiplyVectorScalar", MultiplyVectorScalar},
        {"AddVectors", AddVectors},
        {"MultiplyEulerScalar", MultiplyEulerScalar},
        {"DiffQuaternion", DiffQuaternion},
        {"SlerpUnclampedQuaternion", SlerpUnclampedQuaternion},
    };

    public Dictionary<string, Type> PropsToTypes = new Dictionary<string, Type>
    {
        {"localPosition", typeof(Vector3)},
        {"localRotation", typeof(Quaternion)},
        {"localScale", typeof(Vector3)},
    };

    // Property Names => Functions/Params
    public Dictionary<string, object> PropsToFunctions = new Dictionary<string, object>();
    // { "position": { "fn": "AddVectors", "args": [1, 2.4, ...] } }

    public void AssignFnToProp(string prop, string fn)
    {
        if (!PropsToFunctions.ContainsKey(prop))
        {
            PropsToFunctions[prop] = new Dictionary<string, object>();
        }

        var nextProp = PropsToFunctions.Get<Dictionary<string, object>>(prop);

        if (!nextProp.ContainsKey("fn") || !fn.Equals(nextProp["fn"]))
        {
            var numArgs = NamesToFunctions[fn].GetType().GetGenericArguments().Length - 1; // last arg is return val
            nextProp["fn"] = fn;
            nextProp["args"] = new Ref[numArgs];
        }
    }

    private Dictionary<string, object> GetPropToFn(string prop)
    {
        return PropsToFunctions.Get<Dictionary<string, object>>(prop);
    } 

    public string GetAssignedFn(string prop)
    {
        return GetPropToFn(prop).Get<string>("fn");
    }

    public Ref[] GetArgs(string prop)
    {
        return GetPropToFn(prop).Get<Ref[]>("args");
    }

    public void SetArg(string prop, int paramIndex, Ref val)
    {
        GetArgs(prop)[paramIndex] = val;
    }

    void Start ()
    {

    }
	
	void Update ()
	{
	    InvokeFnWhenReady("localPosition", val => transform.localPosition = (Vector3) val);
	    InvokeFnWhenReady("localRotation", val => transform.localRotation = (Quaternion) val);
	    InvokeFnWhenReady("localScale", val => transform.localScale = (Vector3) val);
    }

    private void InvokeFnWhenReady(string prop, Action<object> setVal)
    {
        if (!PropsToFunctions.ContainsKey(prop)) return;

        var args = GetArgs(prop);
        if (args.Any(a => null == a)) return;

        var fn = GetAssignedFn(prop);
        var derefedArgs = args.Select(a => a.Get()).ToArray();
        var val = NamesToFunctions[fn].DynamicInvoke(derefedArgs);
        setVal(val);
    }
}

public static class DictionaryExtensions
{
    public static T Get<T>(this Dictionary<string, object> instance, string name)
    {
        return (T)instance[name];
    }

}