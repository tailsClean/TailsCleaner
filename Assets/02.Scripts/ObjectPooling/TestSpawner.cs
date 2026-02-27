using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private string poolTag = "Bullet";

    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = false;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private float bulletSpeed = 10f;

    private float _timer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) { Spawn(); }

        if (autoSpawn)
        {
            _timer += Time.deltaTime;
            if (_timer >= spawnInterval)
            {
                Spawn();
                _timer = 0f;
            }
        }
    }

    private void Spawn()
    {
        ObjectPoolManager.Instance.GetAuto(poolTag, transform.position, transform.rotation);
    }
}