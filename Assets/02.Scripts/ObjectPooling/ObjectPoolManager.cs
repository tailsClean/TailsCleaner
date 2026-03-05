using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    // 싱글톤: 어디서든 ObjectPoolManager.Instance로 접근 가능
    public static ObjectPoolManager Instance { get; private set; }

    // 프리팹의 InstanceID를 키로 사용하여 풀 관리
    private Dictionary<int, object> _pools = new Dictionary<int, object>();
    private Dictionary<float, WaitForSeconds> _waitDict = new Dictionary<float, WaitForSeconds>();

    private void Awake() => Instance = this;

    // 외부에서 미리 풀을 만들어두고 싶을 때 호출
    public void CreatePool<T>(T prefab, int defaultSize = 10, int maxSize = 20) where T : Object
    {
        GetPool(prefab, defaultSize, maxSize);
    }

    // 풀이 있으면 가져오고, 없으면 새로 설정하여 반환
    private ObjectPool<T> GetPool<T>(T prefab, int defaultSize = 10, int maxSize = 20) where T : Object
    {
        int id = prefab.GetInstanceID(); // 프리팹별 고유 번호 추출

        if (!_pools.TryGetValue(id, out var pool))
        {
            // 해당 프리팹을 위한 새로운 풀 생성 규칙 정의
            var newPool = new ObjectPool<T>(
                createFunc: () => Instantiate(prefab, transform), // 부족하면 새로 만드는 법
                actionOnGet: (obj) => SetActive(obj, true),       // 꺼낼 때 할 일
                actionOnRelease: (obj) => SetActive(obj, false),  // 넣을 때 할 일
                actionOnDestroy: (obj) => {
                    if (obj is GameObject go) Destroy(go);
                    else if (obj is Component comp) Destroy(comp.gameObject);
                },
                defaultCapacity: defaultSize, // 초기 용량
                maxSize: maxSize  // 최대 저장량
            );
            _pools.Add(id, newPool);
            return newPool;
        }
        return (ObjectPool<T>)pool;
    }

    // 객체 활성화/비활성화 헬퍼 메서드
    private void SetActive<T>(T obj, bool isActive) where T : Object
    {
        if (obj is GameObject go) go.SetActive(isActive);
        else if (obj is Component comp) comp.gameObject.SetActive(isActive);
    }

    // 오브젝트가 있으면 꺼내고 없으면 생성
    public T Get<T>(T prefab, Vector3 pos, Quaternion rot) where T : Object
    {
        var pool = GetPool(prefab);
        var obj = pool.Get();

        // 위치 및 회전 설정
        Transform targetTrans = (obj is GameObject go) ? go.transform : (obj as Component).transform;
        targetTrans.SetPositionAndRotation(pos, rot);

        // Rigidbody2D 속도 초기화 
        if (targetTrans.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        return obj;
    }

    // 객체 반납 (사용이 끝난 객체를 다시 풀에 넣기)
    public void Release<T>(T prefab, T instance) where T : Object
    {
        var pool = GetPool(prefab);
        pool.Release(instance);
    }

    // 'new WaitForSeconds'를 반복하지 않도록 캐싱된 객체 반환
    public WaitForSeconds GetWait(float seconds)
    {
        if (!_waitDict.TryGetValue(seconds, out var wait))
            _waitDict[seconds] = wait = new WaitForSeconds(seconds);
        return wait;
    }
}