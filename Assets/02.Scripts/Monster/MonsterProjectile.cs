using UnityEngine;

public enum PierceType { DISAPPEAR, PIERCE, REFLECT }

public class MonsterProjectile : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;
    private GameObject ownerMonster;

    [Header("--- 기획 데이터 연동 ---")]
    [Tooltip("투사체 데미지")] public float pattern_damage = 10f;
    [Tooltip("발사 속도")] public float projectile_speed = 10f;
    [Tooltip("탄환 수명 (초)")] public float life_time = 5f;
    [Tooltip("유도탄 여부")] public bool is_homing;
    [Tooltip("충돌 모드: 파괴, 관통, 벽 반사")] public PierceType pierce_type;
    [Tooltip("벽 반사 최대 횟수")] public int reflect_count = 3;
    [Tooltip("0보다 크면 포물선 발사 (높이)")] public float arc_height = 0f;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // 플레이어가 Trigger이므로 투사체도 감지를 위해 IsTrigger를 기본적으로 켜줌
        // (Launch에서 pierce_type에 따라 다시 설정)
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;
    }

    public void Launch(Transform playerTarget, GameObject owner)
    {
        target = playerTarget;
        ownerMonster = owner;
        isInitialized = true;

        // 관통/기본/반사 모두 플레이어(Trigger) 감지를 위해 isTrigger를 true로 유지
        GetComponent<Collider2D>().isTrigger = true;

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

        Destroy(gameObject, life_time);
    }

    void Update()
    {
        // 몬스터(hp <= 0) 사망 시 투사체 즉시 제거
        if (ownerMonster == null)
        {
            Destroy(gameObject);
            return;
        }
    }


    void FixedUpdate()
    {
        if (!isInitialized) return;

        // 반사 직후 잠시 유도/회전 중지
        if (reflectTimer > 0)
        {
            reflectTimer -= Time.fixedDeltaTime;
            UpdateRotation();
            return;
        }

        // 유도 로직
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

        // 반사를 위한 마지막 속도 기록
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

    // 이 메서드에서 모든 충돌을 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player");
        bool isWall = other.CompareTag("Wall");

        if (!isPlayer && !isWall) return;

        // 플레이어 충돌 시 처리
        if (isPlayer)
        {
            IDamageable damageable = other.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(pattern_damage);
                //Debug.Log($"[인터페이스 연동] 플레이어에게 {pattern_damage} 데미지 적용");
            }

            if (pierce_type != PierceType.PIERCE)
            {
                Destroy(gameObject);
            }
            return;
        }

        // 2. 벽 충돌 시 처리
        if (isWall)
        {
            if (pierce_type == PierceType.REFLECT && currentBounce < reflect_count)
            {
                HandleReflection(other);
            }
            else if (pierce_type != PierceType.PIERCE)
            {
                // 일반 탄환이나 반사 횟수 초과 시 소멸
                Destroy(gameObject);
            }
        }
    }

    // 반사 계산 함수
    private void HandleReflection(Collider2D wall)
    {
        Vector2 closestPoint = wall.ClosestPoint(transform.position);
        Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;
        Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal);

        rb2D.linearVelocity = reflectDir * projectile_speed;
        reflectTimer = 0.15f;
        currentBounce++;
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