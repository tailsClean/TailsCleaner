using UnityEngine;

public class MonsterShooter : MonoBehaviour
{
    public GameObject projectilePrefab; // 탄환 프리팹
    public Transform firePoint;         // 탄환이 나갈 위치
    public Transform playerTarget;      // 플레이어 위치


    [Header("--- 공격 설정 ---")]
    public float fireRate = 2.0f;       // 2초마다 발사
    private float nextFireTime = 0f;    // 다음 총알이 나가기까지의 시간

    void Start()
    {
        playerTarget = FindAnyObjectByType<PlayerBase>()?.transform;
    }

    public void Update()
    {
        // 방법 1: 일정 시간마다 자동으로 Shoot() 호출
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    
    public void Shoot()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        // 타겟이 없으면 발사 취소
        if (projectilePrefab == null || firePoint == null || playerTarget == null) return;

        // 플레이어 방향 벡터 계산
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;

        // 생성 위치 계산
        float offsetDistance = 1.0f; // 몬스터의 반지름보다 조금 더 크게 설정
        Vector3 spawnPos = transform.position + (Vector3)(dirToPlayer * offsetDistance);

        // 보정된 위치에서 생성
        GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        MonsterProjectile projectile = go.GetComponent<MonsterProjectile>();
        if (projectile != null)
        {
            projectile.Launch(playerTarget);
        }
    }
}