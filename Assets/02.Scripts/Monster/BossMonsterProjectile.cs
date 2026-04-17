using UnityEngine;
using System;

public class BossMonsterProjectile : PoolObject
{
    // 기획서의 PIERCE_TYPE과 호환되도록 유지하되, 내부 로직은 기존의 Flags 방식을 따름
    private Rigidbody2D rb2D;
    private Transform target;
    private int currentBounce = 0;
    private Vector2 lastVelocity;
    private bool isInitialized = false;
    private float reflectTimer = 0f;
    private bool isHomingActive = false; // 포물선 비행 중 유도 활성화 여부
    private float finalDamage;
    private Camera mainCam;

    [Header("--- 프리팹 원본 참조 ---")]
    public GameObject originPrefab;

    [Header("--- 기획 데이터 연동 변수 ---")]
    public PROJECTILE_TYPE projectile_type;
    public float projectile_speed = 10f;
    public float life_time = 5f;
    public bool is_homing;
    public float homing_steer_strength = 5f; // 기획서 외 추가 제어 변수
    public PIERCE_TYPE pierce_type;          // Pattern.cs의 Enum 사용
    public int reflect_count = 3;
    public float arc_height = 0f;
    public float projectile_size = 1f;

    void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().isTrigger = true;

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

    /// <summary>
    /// 패턴 SO 데이터를 투사체에 직접 주입합니다.
    /// </summary>
    public void ApplyProjectileData(Pattern data, float monsterPower)
    {
        this.projectile_type = data.projectile_type;
        this.projectile_speed = data.projectile_speed;
        this.life_time = data.life_time;
        this.is_homing = data.follow;       
        this.pierce_type = data.pierce_type;
        this.arc_height = data.arc_height;
        this.projectile_size = data.projectile_size;

        this.transform.localScale = Vector3.one * data.projectile_size;
        this.finalDamage = monsterPower * data.damage_multiply;
    }

    public void Launch(Transform playerTarget)
    {
        if (playerTarget == null) return;

        target = playerTarget;
        isInitialized = true;

        CancelInvoke(nameof(DeactivateProjectile));
        Invoke(nameof(DeactivateProjectile), life_time);

        if (projectile_type == PROJECTILE_TYPE.Parabola && arc_height > 0)
        {
            ApplyArcShot();
        }
        else
        {
            rb2D.gravityScale = 0;
            Vector2 dir = (target.position - transform.position).normalized;
            rb2D.linearVelocity = dir * projectile_speed;
            isHomingActive = true; // 직선탄은 즉시 유도 활성화 가능
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

        CheckScreenBounds();

        if (is_homing && target != null)
        {
            HandleHomingLogic();
        }

        UpdateRotation();

        if (rb2D.linearVelocity.sqrMagnitude > 0.1f)
            lastVelocity = rb2D.linearVelocity;
    }

    private void CheckScreenBounds()
    {
        // 반사 타입이 아니거나 반사 횟수를 다 썼으면 체크 안 함
        if (pierce_type != PIERCE_TYPE.Reflect || currentBounce >= reflect_count) return;
        if (reflectTimer > 0) return; // 반사 직후 쿨타임 중이면 무시

        // 월드 좌표를 0~1 사이의 화면 좌표로 변환
        Vector3 viewportPos = mainCam.WorldToViewportPoint(transform.position);
        bool reflected = false;
        Vector2 currentVel = rb2D.linearVelocity;
        float margin = 0.03f; // 화면 끝에서 3% 지점을 벽으로 인식 (끼임 방지)

        // 좌우 벽 체크 및 위치 강제 보정
        if (viewportPos.x <= margin)
        {
            currentVel.x = Mathf.Abs(currentVel.x); // 무조건 오른쪽으로
            viewportPos.x = margin + 0.01f;        // 안전지대로 밀어넣기
            reflected = true;
        }
        else if (viewportPos.x >= 1f - margin)
        {
            currentVel.x = -Mathf.Abs(currentVel.x); // 무조건 왼쪽으로
            viewportPos.x = 1f - margin - 0.01f;
            reflected = true;
        }

        // 상하 벽 체크 및 위치 강제 보정
        if (viewportPos.y <= margin)
        {
            currentVel.y = Mathf.Abs(currentVel.y); // 무조건 위쪽으로
            viewportPos.y = margin + 0.01f;
            reflected = true;
        }
        else if (viewportPos.y >= 1f - margin)
        {
            currentVel.y = -Mathf.Abs(currentVel.y); // 무조건 아래쪽으로
            viewportPos.y = 1f - margin - 0.01f;
            reflected = true;
        }

        if (reflected)
        {
            rb2D.linearVelocity = currentVel;
            // 보정된 Viewport 좌표를 다시 월드 좌표로 바꿔서 위치 고정
            transform.position = mainCam.ViewportToWorldPoint(viewportPos);
            currentBounce++;
            reflectTimer = 0.15f; // 반사 직후 짧은 유도 잠금 타임
        }
    }

    private void HandleHomingLogic()
    {
        //  포물선 모드일 때의 특수 처리
        if (projectile_type == PROJECTILE_TYPE.Parabola && arc_height > 0)
        {
            // 아직 상승 중이라면 유도 로직을 실행하지 않고 리턴
            if (!isHomingActive)
            {
                if (rb2D.linearVelocity.y < 0)
                {
                    isHomingActive = true;
                    rb2D.gravityScale = 0; // 유도 시작 시 중력 제거 
                }
                return; // 상승 중에는 아래 유도 계산을 건너뜀
            }
        }
        // 직선탄 모드일 때는 즉시 유도 

        // 실제 유도 계산 
        if (isHomingActive)
        {
            Vector2 desiredDirection = ((Vector2)target.position - rb2D.position).normalized;
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
            if (other.TryGetComponent(out PlayerBase player))
            {
                player.TakeDamage(finalDamage);
            }

            // 관통 모드(Piece)면 통과
            if (pierce_type == PIERCE_TYPE.Piece) return;
        }

        // 벽 반사 로직 
        if (isWall && pierce_type == PIERCE_TYPE.Reflect)
        {
            if (currentBounce < reflect_count)
            {
                Vector2 closestPoint = other.ClosestPoint(transform.position);
                Vector2 normal = ((Vector2)transform.position - closestPoint).normalized;

                if (normal.sqrMagnitude < 0.01f)
                    normal = -lastVelocity.normalized;

                Vector2 reflectDir = Vector2.Reflect(lastVelocity.normalized, normal);
                rb2D.linearVelocity = reflectDir * projectile_speed;

                transform.position = (Vector2)transform.position + (normal * 0.1f);
                reflectTimer = 0.15f;
                currentBounce++;
                return;
            }
        }

        // 아무 조건 미충족 시(Extinction 모드 등) 소멸
        DeactivateProjectile();
    }

    private void ApplyArcShot()
    {
        rb2D.gravityScale = 2.0f;
        Vector3 diff = target.position - transform.position;
        float g = Mathf.Abs(Physics2D.gravity.y * rb2D.gravityScale);

        float vy = Mathf.Sqrt(2 * g * arc_height);
        float time = 2 * vy / g;
        float vx = diff.x / (time == 0 ? 1 : time);

        rb2D.linearVelocity = new Vector2(vx, vy);
    }
}