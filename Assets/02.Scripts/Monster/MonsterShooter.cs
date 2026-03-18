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

    [Header("--- 투사체 데이터 연동 ---")]
    public float projectile_speed = 10f;
    public float projectile_size = 1f;
    public float life_time = 5f;
    public bool is_homing = false;
    public PierceType pierce_type = PierceType.DISAPPEAR;
    public float arc_height = 0f;


    public MonsterState state = MonsterState.MOVE; 
    private float current_cooldown = 0f;
    private bool isPatternReady = false;


    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!enabled) return;
        if (!isPatternReady) return;

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

    public void ApplyProjectilePattern(Pattern patternData)
    {
        if (patternData == null)
        {
            DisableShooter();
            return;
        }

        pattern_cooldown = patternData.cast_time > 0f ? patternData.cast_time : 1f;
        detect_range = patternData.detect_range > 0f ? patternData.detect_range : 10f;
        projectile_count = patternData.projectile_count > 0 ? patternData.projectile_count : 1;
        fire_interval = patternData.fire_interval > 0f ? patternData.fire_interval : 0.2f;

        projectile_speed = patternData.projectile_speed > 0f ? patternData.projectile_speed : 10f;
        projectile_size = patternData.projectile_size > 0f ? patternData.projectile_size : 1f;
        life_time = patternData.life_time > 0f ? patternData.life_time : 5f;
        is_homing = patternData.follow;
        arc_height = patternData.arc_height;

        switch (patternData.pierce_type)
        {
            case PIERCE_TYPE.Extinction:
                pierce_type = PierceType.DISAPPEAR;
                break;
            case PIERCE_TYPE.Piece:
                pierce_type = PierceType.PIERCE;
                break;
            case PIERCE_TYPE.Reflect:
                pierce_type = PierceType.REFLECT;
                break;
        }

        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = true;
        enabled = true;
    }

    public void DisableShooter()
    {
        StopAllCoroutines();
        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = false;
        enabled = false;
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

        if (prefabScript == null)
        {
            Debug.LogError("projectilePrefab에 MonsterProjectile 스크립트가 없습니다!");
            return;
        }

        MonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(prefabScript, spawnPos, Quaternion.identity);
        if (projectile != null)
        {
            projectile.ApplyProjectileData(
                projectile_speed,
                projectile_size,
                life_time,
                is_homing,
                pierce_type,
                arc_height
            );

            projectile.Launch(playerTarget, finalDamage);
        }
    }

}
