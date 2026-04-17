using UnityEngine;
using System.Collections;

public class BossMonsterShooter : MonoBehaviour
{
    [Header("--- 프리팹 설정 ---")]
    public BossMonsterProjectile projectilePrefab;
    public Transform firePoint;
    public Transform playerTarget;

    [Header("--- 패턴 데이터 연동 ---")]
    private Pattern currentPatternData; 
    private float pattern_cooldown;     // 계산된 최종 쿨타임

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
        if (!enabled || !isPatternReady || currentPatternData == null) return;

        // 쿨타임 상시 검사
        if (current_cooldown > 0f)
            current_cooldown -= Time.deltaTime;

        // 쿨타임이 끝났고 플레이어가 사거리 안에 있으면 패턴 시작
        if (state == MonsterState.MOVE && current_cooldown <= 0f)
        {
            // 기획서의 detect_range 참조
            if (playerTarget != null &&
                Vector2.Distance(transform.position, playerTarget.position) <= currentPatternData.detect_range)
            {
                StartCoroutine(BossAttackRoutine());
            }
        }
    }

    /// <summary>
    /// 패턴 SO 데이터를 보스 슈터에 주입합니다.
    /// </summary>
    public void ApplyProjectilePattern(Pattern patternData, float compositionCooldown = -1f)
    {
        if (patternData == null)
        {
            DisableShooter();
            return;
        }

        currentPatternData = patternData;

        pattern_cooldown = compositionCooldown > 0f
            ? compositionCooldown
            : (patternData.cooldown > 0f ? patternData.cooldown : 1f);

        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = true;
        enabled = true;

        Debug.Log($"[BossShooter 연동] ID: {patternData.pattern_id}, 감지범위: {patternData.detect_range}, 쿨타임: {pattern_cooldown}");
    }

    public void DisableShooter()
    {
        StopAllCoroutines();
        currentPatternData = null;
        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = false;
        enabled = false;
    }

    IEnumerator BossAttackRoutine()
    {
        state = MonsterState.PATTERN;

        int count = currentPatternData.projectile_count > 0 ? currentPatternData.projectile_count : 1;
        float interval = currentPatternData.fire_interval > 0f ? currentPatternData.fire_interval : 0.2f;

        for (int i = 0; i < count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(interval);
        }

        state = MonsterState.MOVE;
        current_cooldown = pattern_cooldown;
    }

    public void Shoot()
    {
        if (projectilePrefab == null || playerTarget == null || currentPatternData == null) return;

        BossMonster boss = GetComponent<BossMonster>();
        if (boss == null)
        {
            Debug.LogError("BossMonster 스크립트를 찾을 수 없습니다!");
            return;
        }

        // 발사 위치 설정
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.2f;
        Vector3 spawnPos = (firePoint != null)
            ? firePoint.position
            : transform.position + (Vector3)(dirToPlayer * offsetDistance);

 
        BossMonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(projectilePrefab, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            projectile.ApplyProjectileData(currentPatternData, boss.power);

            // 발사 실행 
            projectile.Launch(playerTarget);
        }
    }
}