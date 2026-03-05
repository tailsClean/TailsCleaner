using UnityEngine;
using System.Collections; 

// 몬스터 상태 정의
public enum MonsterState { MOVE, PATTERN }

public class MonsterShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform playerTarget;

    [Header("--- 기획 데이터 연동 ---")]
    public float pattern_cooldown = 5.0f;     // 기획 1, 11번
    public float detect_range = 10.0f;        // 기획 2번
    public int projectile_count = 3;          // 기획 6번
    public float fire_interval = 0.2f;        // 기획 7번

    public MonsterState state = MonsterState.MOVE; // 기획 4, 10번
    private float current_cooldown = 0f;

    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        // 1번: 쿨타임 상시 검사
        if (current_cooldown > 0)
        {
            current_cooldown -= Time.deltaTime;
        }

        // 1, 2번: 쿨타임이 끝났고 플레이어가 사거리 안에 있으면 패턴 시작
        if (state == MonsterState.MOVE && current_cooldown <= 0)
        {
            if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= detect_range)
            {
                StartCoroutine(AttackPatternRoutine());
            }
        }
    }

    IEnumerator AttackPatternRoutine()
    {
        // 1. 특수 몬스터 베이스를 가져옵니다.
        SpecialBossMonsterBase specialBase = GetComponent<SpecialBossMonsterBase>();

        // 2. 공격 시작 (이동 정지)
        if (specialBase != null)
        {
            specialBase.SetAttackingState(true);
            state = MonsterState.PATTERN; // 슈터 자신의 상태도 갱신
        }

        // 3. 발사 루프
        for (int i = 0; i < projectile_count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(fire_interval);
        }

        // 4. 공격 종료 (이동 재개)
        if (specialBase != null)
        {
            specialBase.SetAttackingState(false);
            state = MonsterState.MOVE;
        }

        current_cooldown = pattern_cooldown;
    }

    public void Shoot()
    {
        if (projectilePrefab == null || playerTarget == null) return;

        // 5, 8번: 플레이어 방향으로 투사체 생성 및 발사
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.0f;
        Vector3 spawnPos = transform.position + (Vector3)(dirToPlayer * offsetDistance);

        MonsterProjectile prefabScript = projectilePrefab.GetComponent<MonsterProjectile>();

        if (prefabScript != null)
        {
            // 제네릭 Spawn<T> 호출: 자동으로 PoolKey를 심어주고 OnSpawn을 실행합니다.
            MonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(prefabScript, spawnPos, Quaternion.identity);

            if (projectile != null)
            {
                projectile.Launch(playerTarget);
            }
        }
        else
        {
            Debug.LogError("projectilePrefab에 MonsterProjectile 스크립트가 없습니다!");
        }
    }
}