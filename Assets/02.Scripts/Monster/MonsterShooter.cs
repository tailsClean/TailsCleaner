using UnityEngine;
using System.Collections;

// 몬스터 상태 정의
public enum MonsterState { MOVE, PATTERN }

public class MonsterShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform playerTarget;

    [Header("--- 기획 데이터 연동 (현재 적용된 패턴) ---")]
    private Pattern currentPatternData; 

    // 로직 제어용 변수들
    public MonsterState state = MonsterState.MOVE;
    private float current_cooldown = 0f;
    private bool isPatternReady = false;

    void Start()
    {
        playerTarget = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (!enabled || !isPatternReady || currentPatternData == null) return;

        // 쿨타임 상시 검사
        if (current_cooldown > 0)
        {
            current_cooldown -= Time.deltaTime;
        }

        // 쿨타임이 끝났고 플레이어가 사거리 안에 있으면 패턴 시작
        if (state == MonsterState.MOVE && current_cooldown <= 0)
        {
            // detect_range 사용
            if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= currentPatternData.detect_range)
            {
                StartCoroutine(AttackPatternRoutine());
            }
        }
    }

    /// <summary>
    /// 패턴 SO 데이터를 슈터에 주입합니다.
    /// </summary>
    public void ApplyProjectilePattern(Pattern patternData)
    {
        if (patternData == null)
        {
            DisableShooter();
            return;
        }

        // 패턴 데이터 통째로 저장
        currentPatternData = patternData;

        // 초기 상태 설정
        current_cooldown = 0f;
        state = MonsterState.MOVE;
        isPatternReady = true;
        enabled = true;
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

    IEnumerator AttackPatternRoutine()
    {
        SpecialBossMonsterBase specialBase = GetComponent<SpecialBossMonsterBase>();

        // 공격 시작 (이동 정지)
        if (specialBase != null)
        {
            specialBase.SetAttackingState(true);
            state = MonsterState.PATTERN;
        }

        // 발사 루프 
        int count = currentPatternData.projectile_count > 0 ? currentPatternData.projectile_count : 1;
        float interval = currentPatternData.fire_interval > 0f ? currentPatternData.fire_interval : 0.2f;

        for (int i = 0; i < count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(interval);
        }

        // 공격 종료 (이동 재개)
        if (specialBase != null)
        {
            specialBase.SetAttackingState(false);
            state = MonsterState.MOVE;
        }

        // 쿨타임 설정 
        current_cooldown = currentPatternData.cast_time > 0f ? currentPatternData.cast_time : 1f;
    }

    public void Shoot()
    {
        if (projectilePrefab == null || playerTarget == null || currentPatternData == null) return;

        SpecialBossMonsterBase monsterBase = GetComponent<SpecialBossMonsterBase>();

        // 몬스터의 기본 파워
        float monsterPower = (monsterBase != null) ? monsterBase.power * monsterBase.type_power_multiply : 10f;

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

        // 오브젝트 풀에서 투사체 소환
        MonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(prefabScript, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            // MonsterProjectile의 ApplyProjectileData 호출 
            projectile.ApplyProjectileData(currentPatternData, monsterPower);

            // 발사 실행
            projectile.Launch(playerTarget);
        }
    }
}