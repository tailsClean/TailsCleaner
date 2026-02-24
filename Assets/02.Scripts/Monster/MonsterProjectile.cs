using UnityEngine;

public enum PierceType { DISAPPEAR, PIERCE, REFLECT }

public class MonsterProjectile : MonoBehaviour
{
    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f; // 반사 직후 유도 로직 일시 정지용

    [Header("--- 기획 데이터 연동 ---")]
    [Tooltip("발사 속도")] public float projectile_speed = 10f;
    [Tooltip("탄환 수명 (초)")] public float life_time = 5f;
    [Tooltip("유도탄 여부")] public bool is_homing;
    [Tooltip("충돌 모드: 파괴, 관통, 벽 반사")] public PierceType pierce_type;
    [Tooltip("벽 반사 최대 횟수")] public int reflect_count = 3;
    [Tooltip("0보다 크면 포물선 발사 (높이)")] public float arc_height = 0f;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
    }

    public void Launch(Transform playerTarget)
    {
        target = playerTarget;
        isInitialized = true;

        // 관통 모드면 유령처럼 통과(Trigger), 아니면 딱딱하게 충돌(Collision)
        if (pierce_type == PierceType.PIERCE)
            GetComponent<Collider2D>().isTrigger = true;
        else
            GetComponent<Collider2D>().isTrigger = false;

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

        // 수명이 다하면 삭제
        Destroy(gameObject, life_time);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

        // 반사 타이머: 벽에 튕긴 직후에는 유도 로직이 방해하지 못하게 함
        if (reflectTimer > 0)
        {
            reflectTimer -= Time.fixedDeltaTime;
            UpdateRotation();
            return;
        }

        // 유도 로직 (직선탄이면서 유도 활성 시)
        if (is_homing && target != null && arc_height <= 0)
        {
            Vector2 direction = ((Vector2)target.position - rb2D.position).normalized;
            float rotateAmount = Vector3.Cross(direction, transform.right).z;

            rb2D.angularVelocity = -rotateAmount * 250f;
            rb2D.linearVelocity = transform.right * projectile_speed;
        }

        UpdateRotation();

        // 반사 계산을 위해 물리 연산 전 속도 저장
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.gameObject, collision);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other.gameObject, null);
    }

    private void HandleHit(GameObject hitObject, Collision2D collision)
    {
        bool isPlayer = hitObject.CompareTag("Player");
        bool isWall = hitObject.CompareTag("Wall");

        if (!isPlayer && !isWall) return;

        // --- 관통(PIERCE) 모드 ---
        if (pierce_type == PierceType.PIERCE)
        {
            // 기획서: 벽과 플레이어를 관통하며 데미지만 주고 유지
            return;
        }

        // --- 반사(REFLECT) 모드 ---
        if (pierce_type == PierceType.REFLECT && isWall)
        {
            if (collision != null && currentBounce < reflect_count)
            {
                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, collision.contacts[0].normal);
                rb2D.linearVelocity = reflectDir * projectile_speed;

                reflectTimer = 0.15f; // 반사 직후 잠시 유도 중지 (끼임 방지)
                currentBounce++;
                Debug.Log($"[반사] {currentBounce}/{reflect_count}");
                return;
            }
        }

        // --- 상단 투척(Arc) 특수 처리 ---
        if (arc_height > 0 && isPlayer)
        {
            // 상단 투척 탄환은 플레이어와 충돌해도 life_time까지 유지되어야 함
            //Debug.Log("[상단투척] 플레이어 충돌 - 파괴 안 함");
            return;
        }

        // --- 기본 모드 (DISAPPEAR 모드 / 반사 횟수 초과 / 일반 탄환 플레이어 적중 시) ---
        //Debug.Log($"[파괴] 원인: {hitObject.name}");
        Destroy(gameObject);
    }

    private void ApplyArcShot()
    {
        rb2D.gravityScale = 2.0f; // 기획 의도에 따른 무게감 설정
        Vector3 diff = target.position - transform.position;
        float g = Physics2D.gravity.y * rb2D.gravityScale;

        // 수직 속도 계산
        float vy = Mathf.Sqrt(-2 * g * arc_height);
        // 체공 시간 계산
        float time = 2 * vy / -g;
        // 수평 속도 계산
        float vx = diff.x / time;

        rb2D.linearVelocity = new Vector2(vx, vy);
    }
}