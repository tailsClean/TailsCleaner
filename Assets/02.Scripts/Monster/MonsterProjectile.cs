using System.Collections;
using UnityEngine;

public class MonsterProjectile : PoolObject, IPullable
{
    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;
    private float finalDamage;

    [Header("--- 프리팹 원본 참조 ---")]
    [Tooltip("반납할 때 필요한 이 투사체의 원본 프리팹")]
    public GameObject originPrefab;

    [Header("--- 기획 데이터 연동 ---")]
    public PROJECTILE_TYPE projectile_type;
    public float projectile_speed;
    public float life_time;
    public bool is_homing;                  // Pattern.follow와 연동
    public PIERCE_TYPE pierce_type;         // Pattern.cs의 Enum 사용
    public float arc_height;
    public float projectile_size;
    [Tooltip("벽 반사 최대 횟수")] public int reflect_count = 3;

    [Header("--- 끌어당기기 설정 ---")]
    [SerializeField] float _pullDuration = 0.1f;
    private Coroutine _pullCoroutine;
    private static readonly WaitForFixedUpdate _waitForFixedUpdate = new();

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // 충돌 감지를 위해 Trigger 설정
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // 데이터 초기화
        currentBounce = 0;
        reflectTimer = 0f;
        isInitialized = false;
        rb2D.linearVelocity = Vector2.zero;
        rb2D.angularVelocity = 0f;
    }

    /// <summary>
    /// 기획 데이터(Pattern SO)를 투사체에 적용합니다.
    /// </summary>
    /// <param name="data">패턴 데이터 객체</param>
    /// <param name="monsterPower">몬스터의 기본 공격력</param>
    public void ApplyProjectileData(Pattern data, float monsterPower)
    {
        // 1. 기획서 필드 매핑
        this.projectile_type = data.projectile_type;
        this.projectile_speed = data.projectile_speed;
        this.life_time = data.life_time;
        this.is_homing = data.follow;         // 기획서 '유도 여부' 컬럼
        this.pierce_type = data.pierce_type;   // 기획서 '관통 타입' 컬럼
        this.arc_height = data.arc_height;
        this.projectile_size = data.projectile_size;

        // 2. 외형 및 데미지 계산 적용
        this.transform.localScale = Vector3.one * data.projectile_size;
        this.finalDamage = monsterPower * data.damage_multiply;
    }

    public void Launch(Transform playerTarget)
    {
        this.target = playerTarget;
        this.isInitialized = true;

        // 수명 후 자동 반납 예약
        CancelInvoke(nameof(DeactivateProjectile));
        Invoke(nameof(DeactivateProjectile), life_time);

        // 발사 로직: 포물선 여부에 따른 분기
        if (projectile_type == PROJECTILE_TYPE.Parabola && arc_height > 0)
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

        if (reflectTimer > 0)
        {
            reflectTimer -= Time.fixedDeltaTime;
            UpdateRotation();
            return;
        }

        // 유도탄 로직: 직선탄 타입이면서 유도 옵션이 켜져 있을 때만 작동
        if (is_homing && target != null && projectile_type == PROJECTILE_TYPE.Straight)
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

        if (isPlayer)
        {
            // 플레이어 데미지 처리
            if (other.TryGetComponent(out PlayerBase player))
            {
                player.TakeDamage(finalDamage);
            }

            // 곡사탄은 플레이어와 충돌 시 삭제하지 않고 통과 (기존 로직 유지)
            if (projectile_type == PROJECTILE_TYPE.Parabola) return;
        }

        // --- 관통/반사/소멸 처리 ---

        // 1. 관통 모드(Piece)
        if (pierce_type == PIERCE_TYPE.Piece) return;

        // 2. 반사 모드(Reflect) + 벽 충돌
        if (pierce_type == PIERCE_TYPE.Reflect && isWall)
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

        // 3. 소멸 모드(Extinction) 또는 반사 횟수 초과 시
        DeactivateProjectile();
    }

    private void ApplyArcShot()
    {
        rb2D.gravityScale = 2.0f;
        Vector3 diff = target.position - transform.position;
        float g = Physics2D.gravity.y * rb2D.gravityScale;

        float vy = Mathf.Sqrt(-2 * g * arc_height);
        float time = 2 * vy / -g;
        float vx = diff.x / (time == 0 ? 1 : time);

        rb2D.linearVelocity = new Vector2(vx, vy);
    }

    // --- IPullable 및 이동 로직 (기존 유지) ---
    public void Pull(Vector2 targetPosition)
    {
        Vector2 startPos = rb2D.position;
        Vector2 randomOffset = Random.insideUnitCircle * 0.05f;
        Vector2 finalTargetPos = targetPosition + randomOffset;
        Vector2 diff = finalTargetPos - startPos;

        if (diff.sqrMagnitude <= 0.001f) return;

        if (_pullCoroutine != null)
            StopCoroutine(_pullCoroutine);

        _pullCoroutine = StartCoroutine(PullCoroutine(startPos, finalTargetPos));
    }

    private IEnumerator PullCoroutine(Vector2 startPos, Vector2 targetPos)
    {
        yield return MoveCoroutine(startPos, targetPos, _pullDuration, null);
        _pullCoroutine = null;
    }

    private IEnumerator MoveCoroutine(Vector2 startPos, Vector2 targetPos, float duration, System.Action<Vector2> onMove)
    {
        float time = 0f;
        while (time < duration)
        {
            time += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            Vector2 nextPos = Vector2.Lerp(startPos, targetPos, t);

            rb2D.MovePosition(nextPos);
            onMove?.Invoke(nextPos);

            yield return _waitForFixedUpdate;
        }
        rb2D.MovePosition(targetPos);
    }
}