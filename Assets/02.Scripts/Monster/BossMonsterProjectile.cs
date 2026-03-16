using UnityEngine;
using System;

public class BossMonsterProjectile : PoolObject
{
    [Flags]
    public enum PierceType
    {
        DISAPPEAR = 0,      // 기본 모드: 충돌 시 즉시 소멸
        PIERCE = 1 << 0,    // 관통 모드: 충돌 무시하고 통과
        REFLECT = 1 << 1    // 반사 모드: 벽에 부딪히면 튕김
    }

    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;
    private bool isHomingActive = false; // 포물선 비행 중 유도 활성화 여부
    private float finalDamage; // 계산된 데미지 저장

    [Header("--- 프리팹 원본 참조 ---")]
    public GameObject originPrefab;

    [Header("--- 기획 데이터 연동 ---")]
    [Tooltip("발사 속도 (직선 발사 시 사용)")] public float projectile_speed = 10f;
    [Tooltip("탄환 수명 (초)")] public float life_time = 5f;
    [Tooltip("유도탄 여부")] public bool is_homing;
    [Tooltip("유도 회전 강도 (높을수록 급격히 꺾임)")] public float homing_steer_strength = 5f;

    [Tooltip("충돌 모드")]
    public PierceType pierce_flags = PierceType.DISAPPEAR;

    [Tooltip("벽 반사 최대 횟수")] public int reflect_count = 3;
    [Tooltip("0보다 크면 포물선 발사 (높이)")] public float arc_height = 0f;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        // 트리거 체크 
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;

        // 연속 충돌 검사 모드로 설정 (빠른 탄환 통과 방지)
        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        currentBounce = 0;
        reflectTimer = 0f;
        isInitialized = false;
        isHomingActive = false;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
        rb2D.gravityScale = 0f;
    }

    public void Launch(Transform playerTarget, float calculatedDamage)
    {
        if (playerTarget == null) return;

        this.finalDamage = calculatedDamage;
        target = playerTarget;
        isInitialized = true;

        CancelInvoke(nameof(DeactivateProjectile));
        Invoke(nameof(DeactivateProjectile), life_time);

        if (arc_height > 0)
        {
            ApplyArcShot();
        }
        else
        {
            rb2D.gravityScale = 0;
            Vector2 dir = (target.position - transform.position).normalized;
            rb2D.linearVelocity = dir * projectile_speed;
            isHomingActive = true;
        }
    }

    private void DeactivateProjectile()
    {
        ObjectPoolManager.Instance.ReturnObject(this);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

        // 반사 직후 짧은 시간 동안은 로직 정지 (끼임 방지)
        if (reflectTimer > 0)
        {
            reflectTimer -= Time.fixedDeltaTime;
            UpdateRotation();
            return;
        }

        if (is_homing && target != null)
        {
            HandleHomingLogic();
        }

        UpdateRotation();

        if (rb2D.linearVelocity.sqrMagnitude > 0.1f)
            lastVelocity = rb2D.linearVelocity;
    }

    private void HandleHomingLogic()
    {
        // 포물선 모드일 경우: 하강하기 시작할 때부터 유도 활성화
        if (arc_height > 0 && !isHomingActive)
        {
            if (rb2D.linearVelocity.y < 0)
            {
                isHomingActive = true;
                rb2D.gravityScale = 0; // 유도 시작 시 중력 영향 제거
            }
        }

        if (isHomingActive)
        {
            // 유도
            Vector2 desiredDirection = ((Vector2)target.position - rb2D.position).normalized;

            // 현재 속도 방향에서 타겟 방향으로 조금씩 회전 
            Vector2 newDirection = Vector2.Lerp(rb2D.linearVelocity.normalized, desiredDirection, Time.fixedDeltaTime * homing_steer_strength);

            rb2D.linearVelocity = newDirection.normalized * projectile_speed;
        }
    }

    private void UpdateRotation()
    {
        if (rb2D.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb2D.linearVelocity.y, rb2D.linearVelocity.x) * Mathf.Rad2Deg;
            rb2D.MoveRotation(angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player");
        bool isWall = other.CompareTag("Wall");

        if (!isPlayer && !isWall) return;

        if (isPlayer)
        {
            Debug.Log($"플레이어 피격! 적용 데미지: {finalDamage}");

            // 관통 로직이 있으면 리턴하여 소멸 방지
            if (pierce_flags.HasFlag(PierceType.PIERCE)) return;
        }

        // 벽 반사 로직
        if (isWall && pierce_flags.HasFlag(PierceType.REFLECT))
        {
            if (currentBounce < reflect_count)
            {
                Vector2 closestPoint = other.ClosestPoint(transform.position);
                Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;

                if (normal.sqrMagnitude < 0.01f)
                    normal = -lastVelocity.normalized;

                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal);

                
                rb2D.linearVelocity = reflectDir * projectile_speed;

                reflectTimer = 0.15f;
                currentBounce++;
                return;
            }
        }

        // 관통 로직
        if (pierce_flags.HasFlag(PierceType.PIERCE) && isPlayer)
        {
            // 플레이어 데미지 처리 로직을 여기에 추가 
            return;
        }

        // 아무 플래그도 없거나 조건 미충족 시 소멸
        DeactivateProjectile();
    }

    private void ApplyArcShot()
    {
        rb2D.gravityScale = 2.0f;

        Vector3 diff = target.position - transform.position;
        float g = Mathf.Abs(Physics2D.gravity.y * rb2D.gravityScale);

        // 최고 도달 높이에 따른 수직 속도 계산
        float vy = Mathf.Sqrt(2 * g * arc_height);

        // 체공 시간 계산
        float time = 2 * vy / g;

        // 시간에 따른 수평 속도 계산
        float vx = diff.x / time;

        rb2D.linearVelocity = new Vector2(vx, vy);
    }
}