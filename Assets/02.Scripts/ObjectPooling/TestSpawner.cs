using UnityEngine;

public class TestSpawner : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private string poolTag = "Bullet"; // 매니저에 등록한 태그와 일치해야 함

    [Header("Spawn Settings")]
    [SerializeField] private bool autoSpawn = false;    // 자동으로 생성할지 여부
    [SerializeField] private float spawnInterval = 0.5f; // 자동 생성 간격
    [SerializeField] private float bulletSpeed = 10f;    // 발사 속도 (물리 테스트용)

    private float _timer;

    void Update()
    {
        // 수동 테스트: 마우스 왼쪽 클릭 시 생성
        if (Input.GetMouseButtonDown(0))
        {
            Spawn();
        }

        // 자동 테스트: 체크박스 켰을 때 일정 간격으로 생성
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
        // 매니저에서 오브젝트 빌려오기
        GameObject obj = ObjectPoolManager.Instance.Get(poolTag, transform.position, transform.rotation);

        if (obj == null) return;

        // 2D 물리 테스트: 앞으로 발사하기
        if (obj.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            
            rb.linearVelocity = transform.right * bulletSpeed;
        }
    }
}