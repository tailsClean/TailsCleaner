using UnityEngine;
using System.Collections;

public class BossMonsterShooter : MonoBehaviour
{
    [Header("--- 프리팹 설정 ---")]
    public BossMonsterProjectile projectilePrefab; // 보스 전용 투사체 타입
    public Transform firePoint;
    public Transform playerTarget;

    [Header("--- 패턴 데이터 ---")]
    public float pattern_cooldown = 3.0f;
    public float detect_range = 15.0f;
    public int projectile_count = 5;
    public float fire_interval = 0.15f;
    public float damage_multiply = 1.2f;

    [Header("--- 상태 관리 ---")]
    public MonsterState state = MonsterState.MOVE;
    private float current_cooldown = 0f;

    
    void Awake()
    {
       
    }

    void Start()
    {
        // 플레이어 자동 찾기
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) playerTarget = player.transform;
        }
    }

    void Update()
    {
        if (current_cooldown > 0)
            current_cooldown -= Time.deltaTime;

        // 이동 중이고 쿨타임이 끝났으며 사거리 안일 때 패턴 실행
        if (state == MonsterState.MOVE && current_cooldown <= 0)
        {
            if (playerTarget != null && Vector2.Distance(transform.position, playerTarget.position) <= detect_range)
            {
                StartCoroutine(BossAttackRoutine());
            }
        }
    }

    IEnumerator BossAttackRoutine()
    {
        // 공격 시작 (이동 정지 로직 연동)
        state = MonsterState.PATTERN;
        

        // Debug.Log("보스 패턴 시작!");

        // 2. 여러 발 발사
        for (int i = 0; i < projectile_count; i++)
        {
            Shoot();
            yield return new WaitForSeconds(fire_interval);
        }

        // 3. 패턴 종료 및 쿨타임 설정
        
        state = MonsterState.MOVE;
        current_cooldown = pattern_cooldown;
    }

    public void Shoot()
    {
        if (projectilePrefab == null || playerTarget == null) return;

        BossMonster boss = GetComponent<BossMonster>();

        //  boss를 찾지 못했을 경우를 대비한 방어 코드
        if (boss == null)
        {
            Debug.LogError("BossMonster 스크립트를 찾을 수 없습니다!");
            return;
        }

        // 발사 위치 계산
        Vector2 dirToPlayer = (playerTarget.position - transform.position).normalized;
        float offsetDistance = 1.2f;
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + (Vector3)(dirToPlayer * offsetDistance);

        // 오브젝트 풀에서 투사체 소환
        BossMonsterProjectile projectile = ObjectPoolManager.Instance.Spawn(projectilePrefab, spawnPos, Quaternion.identity);

        if (projectile != null)
        {
            float finalDamage = boss.monster_power * this.damage_multiply;

            // 투사체에 최종 결과값 전달 
            projectile.Launch(playerTarget, finalDamage);
        }
    }
}