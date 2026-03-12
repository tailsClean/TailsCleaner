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

        SpecialBossMonsterBase monsterBase = GetComponent<SpecialBossMonsterBase>();

        float finalDamage = 0f;

        if (monsterBase != null)
        {
            finalDamage = monsterBase.power* monsterBase.type_power_multiply* monsterBase.damage_multiply;

            //Debug.Log($"<color=cyan>[총알 발사]</color> 몬스터: {gameObject.name} | " +
            //      $"공식: {monsterBase.power}(기본) * {monsterBase.type_power_multiply}(타입) * {monsterBase.pattern_damage}(패턴) " +
            //      $"= <color=yellow>최종 데미지: {finalDamage}</color>");
        }


        // 발사 위치 설정
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.0f;
        Vector3 spawnPos = transform.position + (Vector3)(dirToPlayer * offsetDistance);

        MonsterProjectile prefabScript = projectilePrefab.GetComponent<MonsterProjectile>();

        if (prefabScript != null)
        {
            // 제네릭 Spawn<T> 호출: 자동으로 PoolKey를 심어주고 OnSpawn을 실행
            MonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(prefabScript, spawnPos, Quaternion.identity);

            if (projectile != null)
            {
                projectile.Launch(playerTarget, finalDamage);
            }
        }
        else
        {
            Debug.LogError("projectilePrefab에 MonsterProjectile 스크립트가 없습니다!");
        }
    }
}