using System.Collections.Generic;
using UnityEngine;
using System;

public class DataManager : MonoBehaviour
{
   private static DataManager instance;

    public static DataManager Instance { get => instance; private set => instance = value; }
    private Dictionary<Type, ScriptableObject> _data = new();
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        var allSo = Resources.LoadAll<ScriptableObject>("Data/ScriptableObjects");
        
        foreach(var so in allSo )
            _data[so.GetType()] = so;
    }

    public T GetSOData<T> () where T: ScriptableObject => _data.TryGetValue(typeof(T), out var so) ? (T)so : null; 
}
