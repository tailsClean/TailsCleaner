using UnityEngine;

public enum PierceType { DISAPPEAR, PIERCE, REFLECT }

public class MonsterProjectile : PoolObject
{
    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;

    [Header("--- 프리팹 원본 참조 ---")]
    [Tooltip("반납할 때 필요한 이 투사체의 원본 프리팹")]
    public GameObject originPrefab;

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

        // 플레이어가 Trigger이므로 투사체도 감지를 위해 IsTrigger를 기본적으로 켜줌
        // (Launch에서 pierce_type에 따라 다시 설정)
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;
    }

    public override void OnSpawn()
    {
        base.OnSpawn(); // 부모 클래스 로직 실행 (필요 시)

        // 데이터 초기화 (재사용 시 이전 데이터가 남아있으면 안 됨)
        currentBounce = 0;
        reflectTimer = 0f;
        isInitialized = false;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;

        // 수명 후 자동 반납 예약 (CancelInvoke는 Launch에서 수행)
    }

    public void Launch(Transform playerTarget)
    {
        target = playerTarget;
        isInitialized = true;

        // 관통/기본/반사 모두 플레이어(Trigger) 감지를 위해 isTrigger를 true로 유지
        //GetComponent<Collider2D>().isTrigger = true;

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

        //Destroy(gameObject, life_time);
    }

    private void DeactivateProjectile()
    {
        // 부모(PoolObject)에 정의된 ReturnToPoolAfter와 유사하게 매니저에 직접 반납
        ObjectPoolManager.Instance.ReturnObject(this);
    }

    void FixedUpdate()
    {
        if (!isInitialized) return;

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

    // 이 메서드에서 모든 충돌을 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player");
        bool isWall = other.CompareTag("Wall");

        if (!isPlayer && !isWall) return;

        // --- 상단 투척(Arc) 특수 처리 ---
        // 곡사탄은 플레이어와 충돌해도 삭제하지 않고 통과시킴
        if (arc_height > 0 && isPlayer)
        {
          
            return;
        }

        // 관통 모드(PIERCE): 무시하고 통과
        if (pierce_type == PierceType.PIERCE) return;

        // 반사 모드(REFLECT) + 벽 충돌: 반사 처리
        if (pierce_type == PierceType.REFLECT && isWall)
        {
            if (currentBounce < reflect_count)
            {
                // 트리거 충돌 시 벽의 법선을 구하기 위한 계산
                Vector2 closestPoint = other.ClosestPoint(transform.position);
                Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;

                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal);
                rb2D.linearVelocity = reflectDir * projectile_speed;

                reflectTimer = 0.15f;
                currentBounce++;
                return; // 반사 성공 시 삭제 안 함
            }
        }

        //   파괴 조건 (여기에 도달하면 무조건 삭제)
        // - DISAPPEAR 모드 (전체)
        // - REFLECT 모드에서 플레이어와 부딪힌 경우 (isWall이 false이므로 2번을 건너뜀)
        // - REFLECT 모드에서 반사 횟수 초과 시
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