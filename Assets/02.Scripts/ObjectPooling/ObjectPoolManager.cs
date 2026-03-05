using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    // 프리팹별로 큐를 관리하는 딕셔너리
    private Dictionary<string, Queue<PoolObject>> _poolDictionary = new Dictionary<string, Queue<PoolObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // [즉시 소환] T는 PoolObject를 상속받은 타입이어야 함
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : PoolObject
    {
        string key = prefab.name;

        if (!_poolDictionary.ContainsKey(key))
        {
            _poolDictionary.Add(key, new Queue<PoolObject>());
        }

        T obj;
        if (_poolDictionary[key].Count > 0)
        {
            obj = _poolDictionary[key].Dequeue() as T;
        }
        else
        {
            obj = Instantiate(prefab, transform);
            obj.PoolKey = key; // 키 값을 부여해 반납 위치를 기억하게 함
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        obj.OnSpawn();

        return obj;
    }

    // [지연 소환 기능] 코루틴 활용
    public void SpawnWithDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay) where T : PoolObject
    {
        StartCoroutine(CoSpawnDelay(prefab, position, rotation, delay));
    }

    private IEnumerator CoSpawnDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay) where T : PoolObject
    {
        yield return new WaitForSeconds(delay);
        Spawn(prefab, position, rotation);
    }

    // 풀로 반납
    public void ReturnObject(PoolObject obj)
    {
        if (obj == null) return;

        if (string.IsNullOrEmpty(obj.PoolKey))
        {
            Debug.LogWarning($"{obj.name} 객체에 PoolKey가 없습니다. 풀링을 통해 생성되지 않았으므로 파괴합니다.");
            Destroy(obj.gameObject);
            return;
        }

        if (!_poolDictionary.ContainsKey(obj.PoolKey))
        {
            // 만약 키는 있는데 딕셔너리에 없다면 새로 생성해줌 (예외 방지)
            _poolDictionary.Add(obj.PoolKey, new Queue<PoolObject>());
        }

        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        _poolDictionary[obj.PoolKey].Enqueue(obj);
    }
}