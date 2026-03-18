using UnityEngine;
using System.Collections;

public class BossMonsterShooter : MonoBehaviour
{
    [Header("--- 프리팹 설정 ---")]
    public BossMonsterProjectile projectilePrefab;
    public Transform firePoint;
    public Transform playerTarget;

    [Header("--- 패턴 데이터 ---")]
    public float pattern_cooldown = 3.0f;
    public float detect_range = 15.0f;
    public int projectile_count = 5;
    public float fire_interval = 0.15f;
    public float damage_multiply = 1.2f;

    [Header("--- 투사체 데이터 ---")]
    public float projectile_speed = 10f;
    public float projectile_size = 1f;
    public float life_time = 5f;
    public bool is_homing = false;
    public BossMonsterProjectile.PierceType pierce_type = BossMonsterProjectile.PierceType.DISAPPEAR;
    public float arc_height = 0f;

    [Header("--- 상태 관리 ---")]
    public MonsterState state = MonsterState.MOVE;
    private float current_cooldown = 0f;
    private bool isPatternReady = false;

    void Start()
    {
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (!enabled) return;
        if (!isPatternReady) return;

        if (current_cooldown > 0f)
            current_cooldown -= Time.deltaTime;

        if (state == MonsterState.MOVE && current_cooldown <= 0f)
        {
            if (playerTarget != null &&
                Vector2.Distance(transform.position, playerTarget.position) <= detect_range)
            {
                StartCoroutine(BossAttackRoutine());
            }
        }
    }

    public void ApplyProjectilePattern(Pattern patternData, float compositionCooldown = -1f)
    {
        if (patternData == null)
        {
            DisableShooter();
            return;
        }

        pattern_cooldown = compositionCooldown > 0f
            ? compositionCooldown
            : (patternData.cooldown > 0f ? patternData.cooldown : 1f);

        detect_range = patternData.detect_range > 0f ? patternData.detect_range : 10f;
        projectile_count = patternData.projectile_count > 0 ? patternData.projectile_count : 1;
        fire_interval = patternData.fire_interval > 0f ? patternData.fire_interval : 0.2f;
        damage_multiply = patternData.damage_multiply > 0f ? patternData.damage_multiply : 1f;

        projectile_speed = patternData.projectile_speed > 0f ? patternData.projectile_speed : 10f;
        projectile_size = patternData.projectile_size > 0f ? patternData.projectile_size : 1f;
        life_time = patternData.life_time > 0f ? patternData.life_time : 5f;
        is_homing = patternData.follow;
        arc_height = patternData.arc_height;

        switch (patternData.pierce_type)
        {
            case PIERCE_TYPE.Extinction:
                pierce_type = BossMonsterProjectile.PierceType.DISAPPEAR;
                break;
            case PIERCE_TYPE.Piece:
                pierce_type = BossMonsterProjectile.PierceType.PIERCE;
                break;
            case PIERCE_TYPE.Reflect:
                pierce_type = BossMonsterProjectile.PierceType.REFLECT;
                break;
        }

        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = true;
        enabled = true;

        Debug.Log(
            $"[BossShooter 적용 완료] " +
            $"PatternId:{patternData.pattern_id}, Logic:{patternData.pattern_logic_type}, " +
            $"Cooldown:{pattern_cooldown}, Detect:{detect_range}, Count:{projectile_count}, Interval:{fire_interval}, " +
            $"DamageMul:{damage_multiply}, Speed:{projectile_speed}, Size:{projectile_size}, Life:{life_time}, " +
            $"Follow:{is_homing}, Pierce:{pierce_type}, Arc:{arc_height}"
        );
    }

    public void DisableShooter()
    {
        StopAllCoroutines();
        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = false;
        enabled = false;
    }

    IEnumerator BossAttackRoutine()
    {
        state = MonsterState.PATTERN;

        for (int i = 0; i < projectile_count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(fire_interval);
        }

        state = MonsterState.MOVE;
        current_cooldown = pattern_cooldown;
    }

    public void Shoot()
    {
        if (projectilePrefab == null || playerTarget == null) return;

        BossMonster boss = GetComponent<BossMonster>();
        if (boss == null)
        {
            Debug.LogError("BossMonster 스크립트를 찾을 수 없습니다!");
            return;
        }

        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.2f;
        Vector3 spawnPos = (firePoint != null)
            ? firePoint.position
            : transform.position + (Vector3)(dirToPlayer * offsetDistance);

        BossMonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(projectilePrefab, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            float finalDamage = boss.power * damage_multiply;

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