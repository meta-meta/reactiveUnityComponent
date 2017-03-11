using System;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveComponent : MonoBehaviour {
    public Dictionary<string, object> State = new Dictionary<string, object>
    {
        {"", 1}
    };

    public void SetState(string prop, string param, object val)
    {
        var exists = this.State.ContainsKey(prop);
        var nextProp = exists
            ? (Dictionary<string, object>) this.State.Get<Dictionary<string, object>>(prop)
            : new Dictionary<string, object>();
        nextProp[param] = val;
        this.State[prop] = nextProp;
    }

    private static Func<Vector3, float, Func<Vector3>> multiplyVectorScalar = (Vector3 v, float f) => () => v*f;
    private static Func<Vector3, Vector3, Func<Vector3>> addVectors = (Vector3 v1, Vector3 v2) => () => v1 + v2;
    private static Func<Quaternion, float, Func<Quaternion>> multiplyRotation = (Quaternion q, float f) => () => Quaternion.Euler(q.eulerAngles * f);
    
    public Dictionary<string, Delegate> Functions = new Dictionary<string, Delegate>
    {
        {"multiplyVectorScalar", multiplyVectorScalar},
        {"addVectors", addVectors},
        {"multiplyRotation", multiplyRotation}
    };

    public Dictionary<string, Type> PropertiesToFunctions = new Dictionary<string, Type>
    {
        {"localPosition", typeof(Func<Vector3>)},
        {"localRotation", typeof(Func<Quaternion>)}
    };

    /*
     * { position: Func<Vector3, Vector3> }
             */

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