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
    public float pattern_cooldown = 5.0f;     
    public float detect_range = 10.0f;        
    public int projectile_count = 3;          
    public float fire_interval = 0.2f;        

    public MonsterState state = MonsterState.MOVE; 
    private float current_cooldown = 0f;

    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        // 쿨타임 상시 검사
        if (current_cooldown > 0)
        {
            current_cooldown -= Time.deltaTime;
        }

        // 쿨타임이 끝났고 플레이어가 사거리 안에 있으면 패턴 시작
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
        // 특수 몬스터 베이스를 가져옵니다.
        SpecialBossMonsterBase specialBase = GetComponent<SpecialBossMonsterBase>();

        // 공격 시작 (이동 정지)
        if (specialBase != null)
        {
            specialBase.SetAttackingState(true);
            state = MonsterState.PATTERN; // 슈터 자신의 상태도 갱신
        }

        // 발사 루프
        for (int i = 0; i < projectile_count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(fire_interval);
        }

        // 공격 종료 (이동 재개)
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
        
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.0f;
        Vector3 spawnPos = transform.position + (Vector3)(dirToPlayer * offsetDistance);

        GameObject go = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        MonsterProjectile projectile = go.GetComponent<MonsterProjectile>();

        if (projectile != null)
        {
            projectile.Launch(playerTarget, gameObject);
        }
    }
}