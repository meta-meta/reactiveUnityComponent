using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityScript.Lang;

public class ReactiveComponent : MonoBehaviour
{
    private static Func<Vector3, float, Vector3> multiplyVectorScalar = (Vector3 v, float f) => v * f;
    private static Func<Vector3, Vector3, Vector3> addVectors = (Vector3 v1, Vector3 v2) => v1 + v2;
    private static Func<Quaternion, float, Quaternion> multiplyRotation = (Quaternion q, float f) => Quaternion.Euler(q.eulerAngles * f);

    public Dictionary<string, Delegate> Functions = new Dictionary<string, Delegate>
    {
        {"multiplyVectorScalar", multiplyVectorScalar},
        {"addVectors", addVectors},
        {"multiplyRotation", multiplyRotation}
    };

    public Dictionary<string, Type> PropsToFunctionTypes = new Dictionary<string, Type>
    {
        {"localPosition", typeof(Vector3)},
        {"localRotation", typeof(Quaternion)}
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
            var numArgs = Functions[fn].GetType().GetGenericArguments().Length - 1; // last arg is return val
            Debug.Log("assigning " + fn + "(<" + numArgs + ">) to " + prop);
            nextProp["fn"] = fn;
            nextProp["args"] = new Ref[numArgs];
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

    public Ref[] GetArgs(string prop)
    {
        return GetFnAssignment(prop).Get<Ref[]>("args");
    }

    public void SetArg(string prop, int paramIndex, Ref val)
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
	void Update ()
	{
	    if (PropsToFunctions.ContainsKey("localPosition"))
	    {
	        var p = GetFnAssignment("localPosition");
	        var args = GetArgs("localPosition");
	        if (args.All(a => null != a))
	        {
	            var fn = p.Get<string>("fn");
	            var derefedArgs = args.Select(a => a.Get()).ToArray();
	            var val = Functions[fn].DynamicInvoke(derefedArgs);
                transform.localPosition = (Vector3)val;
            }
        }

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