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

    // 내부 공통 생성 로직
    private T GetOrCreateObject<T>(T prefab) where T : PoolObject
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
            obj.PoolKey = key;
        }

        return obj;
    }

    // 기본 Spawn
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : PoolObject
    {
        T obj = GetOrCreateObject(prefab);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        obj.OnSpawn();

        return obj;
    }

    // 몬스터 ID를 먼저 넣고 스폰하는 오버로드
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation, int monsterId) where T : PoolObject
    {
        T obj = GetOrCreateObject(prefab);

        obj.transform.SetPositionAndRotation(position, rotation);

        if (obj.TryGetComponent<MonsterBase>(out var monster))
        {
            monster.SetMonsterId(monsterId);
        }

        obj.gameObject.SetActive(true);
        obj.OnSpawn();

        return obj;
    }

    // 지연 소환 기능
    public void SpawnWithDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay) where T : PoolObject
    {
        StartCoroutine(CoSpawnDelay(prefab, position, rotation, delay));
    }

    // 몬스터 ID 포함 지연 소환 기능
    public void SpawnWithDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay, int monsterId) where T : PoolObject
    {
        StartCoroutine(CoSpawnDelay(prefab, position, rotation, delay, monsterId));
    }

    private IEnumerator CoSpawnDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay) where T : PoolObject
    {
        yield return new WaitForSeconds(delay);
        Spawn(prefab, position, rotation);
    }

    private IEnumerator CoSpawnDelay<T>(T prefab, Vector3 position, Quaternion rotation, float delay, int monsterId) where T : PoolObject
    {
        yield return new WaitForSeconds(delay);
        Spawn(prefab, position, rotation, monsterId);
    }

    // 풀로 반납
    public void ReturnObject(PoolObject obj)
    {
        if (obj == null) return;

        if (string.IsNullOrEmpty(obj.PoolKey))
        {
            //Debug.LogWarning($"{obj.name} 객체에 PoolKey가 없습니다. 풀링을 통해 생성되지 않았으므로 파괴합니다.");
            Destroy(obj.gameObject);
            return;
        }

        if (!_poolDictionary.ContainsKey(obj.PoolKey))
        {
            _poolDictionary.Add(obj.PoolKey, new Queue<PoolObject>());
        }

        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        _poolDictionary[obj.PoolKey].Enqueue(obj);
    }
}