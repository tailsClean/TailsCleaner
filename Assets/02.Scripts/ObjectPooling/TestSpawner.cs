using UnityEngine;
using System.Collections;

public class TestSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private PoolObject prefab;
    [SerializeField] private int defaultSize = 20;

    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 0.5f;

    private float spawnDelay = 1f;

    void Start()
    {
        // ObjectPoolManager.Instance.CreatePool(prefab, defaultSize);
    }

    void Update()
    {
        // 즉시 스폰 (마우스 좌클릭)
        if (Input.GetMouseButtonDown(0))
        {
            Spawn();
        }

        // 딜레이 스폰 (스페이스바)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    private void Spawn()
    {
        if (prefab == null) return;

        ObjectPoolManager.Instance.Spawn(prefab, transform.position, Quaternion.identity);
    }

    // 지정한 spawnDelay 시간만큼 대기한 뒤 Spawn()을 실행
    private IEnumerator SpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }
}