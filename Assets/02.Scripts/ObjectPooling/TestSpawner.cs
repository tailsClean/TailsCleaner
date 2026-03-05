using UnityEngine;
using System.Collections;

public class TestSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int defaultSize = 20;

    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 0.5f;

    private float spawnDelay = 1f;

    void Start()
    {
        // Transform 대신 GameObject로 풀 생성
        ObjectPoolManager.Instance.CreatePool<GameObject>(prefab, defaultSize);
    }

    void Update()
    {
        // 즉시 스폰
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("<color=cyan>[Test]</color> 마우스 좌클릭 감지!");
            Spawn();
        }

        // 딜레이 스폰
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Debug.Log("<color=yellow>[Test]</color> 딜레이 스폰 시작!");
            StartCoroutine(SpawnWithDelay());
        }

    }

    private void Spawn()
    {
        // ObjectPoolManager에서 prefab을 풀에서 꺼내기
        // Transform 타입으로 가져오고, 현재 위치(transform.position)와 회전(Quaternion.identity) 적용
        var monster = ObjectPoolManager.Instance.Get<Transform>(prefab.transform, transform.position, Quaternion.identity);

        // 가져온 오브젝트가 null이 아니고, PoolObject 컴포넌트를 가지고 있다면
        if (monster != null && monster.TryGetComponent<PoolObject>(out var po))
        {
            // po.Setup(): PoolObject에 Release 콜백 등록
            // 나중에 사용이 끝나면 ObjectPoolManager.Release()를 통해 다시 풀에 반납
            // Release 호출 시 (프리팹, 인스턴스) 두 개 인자를 넣어야함
            po.Setup(obj => ObjectPoolManager.Instance.Release<Transform>(prefab.transform, obj.transform));
        }
    }

    // 지정한 spawnDelay 시간만큼 대기한 뒤 Spawn()을 실행
    private IEnumerator SpawnWithDelay()
    {
        yield return ObjectPoolManager.Instance.GetWait(spawnDelay);
        Spawn();
    }
}