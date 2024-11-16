using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        Instance = this as T;
    }

}

public abstract class Singleton<T> : StaticSingleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null)
        {
            //Destroy(gameObject)
            base.Awake();
        }
    }
}

