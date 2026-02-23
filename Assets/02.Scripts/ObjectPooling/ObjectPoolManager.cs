using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public class PoolInfo
    {
        public string tag;
        public GameObject prefab;
        public int defaultCapacity = 10;
        public int maxSize = 100;
    }

    public List<PoolInfo> poolInfos;
    private Dictionary<string, IObjectPool<GameObject>> _poolDict = new Dictionary<string, IObjectPool<GameObject>>();

    void Awake()
    {
        Instance = this;

        foreach (var info in poolInfos)
        {
            // 변수 오염을 방지
            var targetInfo = info;

            IObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateNewObject(targetInfo.prefab, targetInfo.tag), // 인자를 여기서 넘김
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyObject,
                collectionCheck: true,
                defaultCapacity: targetInfo.defaultCapacity,
                maxSize: targetInfo.maxSize
            );

            _poolDict.Add(targetInfo.tag, pool);
        }
    }

    private static int _debugCounter = 0; // 전역 카운터

    // --- 풀링 이벤트 함수들 ---
    private GameObject CreateNewObject(GameObject prefab, string tag)
    {
        Debug.Log($"<color=yellow>[Pool]</color> {tag} 오브젝트 신규 생성!");

        GameObject obj = Instantiate(prefab);
        obj.name = tag;
        obj.transform.SetParent(this.transform); // 매니저 자식으로 생성
        return obj;
    }

    private void OnGetFromPool(GameObject obj)
    {
        Debug.Log($"<color=cyan>[Pool]</color> 창고에서 {obj.name} 꺼냄 (재사용)");

        obj.SetActive(true);

        if (obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnReleaseToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    private void OnDestroyObject(GameObject obj)
    {
        Destroy(obj);
    }

    // --- 외부 호출용 메서드 ---
    public GameObject Get(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDict.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist!");
            return null;
        }

        GameObject obj = _poolDict[tag].Get();
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    public void Release(string tag, GameObject obj)
    {
        if (_poolDict.ContainsKey(tag))
        {
            _poolDict[tag].Release(obj);
        }
        else
        {
            // 태그가 없는데 반납하려 할 경우 파괴 처리해서 메모리 누수 방지
            Destroy(obj);
        }
    }
}