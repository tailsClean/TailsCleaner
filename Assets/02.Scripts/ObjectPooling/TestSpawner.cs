using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

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
        // 2. 새로운 Input System 방식으로 체크
        // 마우스 왼쪽 버튼 클릭
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Spawn();
        }

        // 스페이스바 입력
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
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