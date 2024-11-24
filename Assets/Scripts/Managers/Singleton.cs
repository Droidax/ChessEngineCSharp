using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StaticSingleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();

                if (_instance == null)
                {
                    GameObject newGameObject = new GameObject("Auto-generated " + typeof(T));
                    newGameObject.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

}

public abstract class Singleton<T> : StaticSingleton<T> where T : MonoBehaviour, new()
{
    //implement
}

