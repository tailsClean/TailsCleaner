using UnityEngine;
using System.Collections;

public class TestSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private PoolObject prefab;
    [SerializeField] private int defaultSize = 20;

    [Header("Monster Settings")]
    [SerializeField] private int monsterId = 1;

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
        if (Input.GetMouseButtonDown(0))
        {
            Spawn();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SpawnWithDelay());
        }
    }

    private void Spawn()
    {
        if (prefab == null) return;

        ObjectPoolManager.Instance.Spawn(prefab, transform.position, Quaternion.identity, monsterId);
    }

    private IEnumerator SpawnWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        Spawn();
    }
}