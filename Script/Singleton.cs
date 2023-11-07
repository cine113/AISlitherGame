using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;

    public static T Instance
    {
        get
        {
            instance = (T)FindObjectOfType(typeof(T));

            if (instance == null)
            {
                var ob = new GameObject(typeof(T).Name, typeof(T));
                instance = ob.GetComponent<T>();
            }

            return instance;
        }
    }

    protected virtual void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
