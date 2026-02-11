using System.Collections.Generic;
using UnityEngine;
using System;

public class DataManager : MonoBehaviour
{
   private static DataManager instance;

    public static DataManager Instance { get => instance; private set => instance = value; }

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

        DataParse();
    }

    #region Data Table
    
    
    #endregion

    private void DataParse()
    {
        
    }

    private void AddToDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict, List<TValue> list, Func<TValue, TKey> keySelector)
    {
        if (list == null) return;

        foreach (var item in list)
        {
            TKey key = keySelector(item);
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, item);
            }
            else
            {
                //Debug.LogWarning($"중복된 키 발견: {key} in {typeof(TValue).Name}");
            }
        }
            
    }

    private void AddToListDictionary<TKey, TValue>(Dictionary<TKey, List<TValue>> dict, List<TValue> list, Func<TValue, TKey> keySelector)
    {
         if (list == null) return;

        foreach (var item in list)
        {
            TKey key = keySelector(item);

            // 키가 없으면 리스트를 새로 생성
            if (!dict.ContainsKey(key))
            {
                dict[key] = new List<TValue>();
            }

            // 해당 키의 리스트에 데이터 추가
            dict[key].Add(item);
        }
    }
}
