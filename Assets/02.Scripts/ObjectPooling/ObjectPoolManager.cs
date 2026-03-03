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

    // 인스펙터에 설정된 정보를 바탕으로 각 태그별 오브젝트 풀을 초기화
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

    // 풀에 객체가 없을 때 새로 생성하고 소속 태그를 설정하는 로직
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

    // 풀에서 꺼낼 때 실행
    private void OnGetFromPool(GameObject obj) => obj.SetActive(true);

    // 풀에 반납할 때 실행
    private void OnReleaseToPool(GameObject obj) => obj.SetActive(false);

    // 풀이 가득 차거나 삭제될 때 실행
    private void OnDestroyPoolObject(GameObject obj) => Destroy(obj);

    // 즉시 소환
    public GameObject Get(string tag, Vector3 position, Quaternion rotation, float customLifeTime = -1f)
    {
        if (!_poolDict.ContainsKey(tag)) return null;

        // 풀에서 꺼내기 (비활성화 상태)
        GameObject obj = _poolDict[tag].Get();

        // 위치/회전 세팅
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 물리 엔진 리셋
        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        return obj;
    }

    // Delay 소환을 위한 로직
    public void GetAuto(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_infoDict.ContainsKey(tag)) return;
        PoolInfo info = _infoDict[tag];

        // 딜레이가 0보다 크면 코루틴으로 지연 소환, 아니면 즉시 소환
        if (info.defaultSpawnDelay > 0)
            StartCoroutine(GetDelayedCoroutine(tag, position, rotation, info.defaultSpawnDelay));
        else
            Get(tag, position, rotation);
    }

    // 설정된 지연 시간(delay)만큼 대기 후, 풀에서 객체 꺼내오는 로직
    private IEnumerator GetDelayedCoroutine(string tag, Vector3 pos, Quaternion rot, float delay)
    {
        yield return new WaitForSeconds(delay);
        Get(tag, pos, rot);
    }

    // 오브젝트 반납
    public void Release(string tag, GameObject obj) => _poolDict[tag].Release(obj);
}