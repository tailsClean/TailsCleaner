using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolInfo
    {
        public string tag;
        public GameObject prefab;
        public int defaultCapacity = 10;
        public int maxSize = 20;
        public float defaultSpawnDelay = 0f;
        public float defaultLifeTime = 2f;
    }

    public static ObjectPoolManager Instance;

    [SerializeField] private List<PoolInfo> poolInfos;
    private Dictionary<string, IObjectPool<GameObject>> _poolDict = new Dictionary<string, IObjectPool<GameObject>>();
    private Dictionary<string, PoolInfo> _infoDict = new Dictionary<string, PoolInfo>();

    private void Awake()
    {
        Instance = this;
        Init();
    }

    private void Init()
    {
        foreach (var info in poolInfos)
        {
            _infoDict[info.tag] = info;
            _poolDict[info.tag] = new ObjectPool<GameObject>(
                () => CreatePooledItem(info),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPoolObject,
                true,
                info.defaultCapacity,
                info.maxSize
            );
        }
    }

    private GameObject CreatePooledItem(PoolInfo info)
    {
        GameObject obj = Instantiate(info.prefab, transform);
        if (obj.TryGetComponent<PoolObject>(out var po))
        {
            po.poolTag = info.tag;
        }
        obj.SetActive(false);
        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        
    }

    private void OnReleaseToPool(GameObject obj) => obj.SetActive(false);
    private void OnDestroyPoolObject(GameObject obj) => Destroy(obj);

    public GameObject Get(string tag, Vector3 position, Quaternion rotation, float customLifeTime = -1f)
    {
        if (!_poolDict.ContainsKey(tag)) return null;

        // 풀에서 꺼내기 (비활성화 상태)
        GameObject obj = _poolDict[tag].Get();

        // 위치/회전 세팅
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 물리 엔진 리셋 (있을 경우)
        if (obj.TryGetComponent<Rigidbody2D>(out var rb)) { rb.linearVelocity = Vector2.zero; }

        // 수명 데이터 주입 및 타이머 시작
        if (obj.TryGetComponent<PoolObject>(out var po))
        {
            float life = (customLifeTime > 0) ? customLifeTime : _infoDict[tag].defaultLifeTime;
            po.ResetReleaseTimer(life);
        }

        
        obj.SetActive(true);

        return obj;
    }

    public void GetAuto(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_infoDict.ContainsKey(tag)) return;
        PoolInfo info = _infoDict[tag];

        if (info.defaultSpawnDelay > 0)
            StartCoroutine(GetDelayedCoroutine(tag, position, rotation, info.defaultSpawnDelay, info.defaultLifeTime));
        else
            Get(tag, position, rotation, info.defaultLifeTime);
    }

    private IEnumerator GetDelayedCoroutine(string tag, Vector3 pos, Quaternion rot, float delay, float life)
    {
        yield return new WaitForSeconds(delay);
        Get(tag, pos, rot, life);
    }

    public void Release(string tag, GameObject obj) => _poolDict[tag].Release(obj);
}