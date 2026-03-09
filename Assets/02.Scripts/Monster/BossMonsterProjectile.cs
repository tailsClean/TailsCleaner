using UnityEngine;
using System; // [Flags] 속성 사용을 위해 필수

public class BossMonsterProjectile : PoolObject
{
    [Flags]
    public enum PierceType
    {
        NONE = 0,           // 아무것도 체크 안 함 (기본 소멸)
        PIERCE = 1 << 0,    // 1 (관통)
        REFLECT = 1 << 1    // 2 (반사)
    }

    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;

    [Header("--- 프리팹 원본 참조 ---")]
    [Tooltip("반납 시 필요한 이 투사체의 원본 프리팹")]
    public GameObject originPrefab;

    [Header("--- 기획 데이터 연동 ---")]
    [Tooltip("발사 속도")] public float projectile_speed = 10f;
    [Tooltip("탄환 수명 (초)")] public float life_time = 5f;
    [Tooltip("유도탄 여부")] public bool is_homing;

    [Tooltip("충돌 모드: 체크박스 다중 선택 (모두 해제 시 부딪히면 삭제)")]
    public PierceType pierce_flags = PierceType.NONE;

    [Tooltip("벽 반사 최대 횟수")] public int reflect_count = 3;
    [Tooltip("0보다 크면 포물선 발사 (높이)")] public float arc_height = 0f;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // 충돌 감지를 위해 IsTrigger 설정
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // 오브젝트 풀링 재사용을 위한 데이터 초기화
        currentBounce = 0;
        reflectTimer = 0f;
        isInitialized = false;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
    }

    public void Launch(Transform playerTarget)
    {
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
        }
    }

    private void DeactivateProjectile()
    {
        ObjectPoolManager.Instance.ReturnObject(this);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

        // 반사 직후 잠시 로직 정지 (벽 끼임 방지)
        if (reflectTimer > 0)
        {
            reflectTimer -= Time.fixedDeltaTime;
            UpdateRotation();
            return;
        }

        if (is_homing && target != null && arc_height <= 0)
        {
            Vector2 direction = ((Vector2)target.position - rb2D.position).normalized;
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            rb2D.angularVelocity = -rotateAmount * 250f;
            rb2D.linearVelocity = transform.right * projectile_speed;
        }
        else
        {
            UpdateRotation();
        }

        if (rb2D.linearVelocity.sqrMagnitude > 0.1f)
            lastVelocity = rb2D.linearVelocity;
    }

    private void UpdateRotation()
    {
        if (rb2D.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb2D.linearVelocity.y, rb2D.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player");
        bool isWall = other.CompareTag("Wall");

        if (!isPlayer && !isWall) return;

        // 곡사탄 플레이어 통과 처리
        if (arc_height > 0 && isPlayer) return;

        // 1. 관통(PIERCE) 여부 확인
        if (pierce_flags.HasFlag(PierceType.PIERCE))
        {
            return; // 관통 설정 시 모든 충돌 무시
        }

        // 2. 반사(REFLECT) 여부 및 벽 충돌 확인
        if (pierce_flags.HasFlag(PierceType.REFLECT) && isWall)
        {
            if (currentBounce < reflect_count)
            {
                Vector2 closestPoint = other.ClosestPoint(transform.position);
                Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;

                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal);
                rb2D.linearVelocity = reflectDir * projectile_speed;

                reflectTimer = 0.15f;
                currentBounce++;
                return;
            }
        }

        // 3. 위 조건에 해당하지 않으면 소멸 (NONE 포함)
        DeactivateProjectile();
    }

    private void ApplyArcShot()
    {
        rb2D.gravityScale = 2.0f;
        Vector3 diff = target.position - transform.position;
        float g = Physics2D.gravity.y * rb2D.gravityScale;

        float vy = Mathf.Sqrt(-2 * g * arc_height);
        float time = 2 * vy / -g;
        float vx = diff.x / time;

        rb2D.linearVelocity = new Vector2(vx, vy);
    }
}