using UnityEngine;
using MonsterEnum;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : PoolObject, IDamageable
{
    [Header("--- 환경 설정 ---")]
    public Transform target;
    public float stoppingDistance = 0.1f;

    [Header("--- 몬스터 정체성 ---")]
    public abstract MonsterType monsterType { get; }

    [Header("--- 기준 스탯 ---")]
    public float hp = 1.0f;
    public float power = 1.0f;
    public float moveSpeed = 1.0f;
    public float hitBox = 1.0f;
    public float mass = 1.0f;
    public float KBResist = 1.0f;

    private bool _baseCached;
    private float _baseHp;
    private float _basePower;

    [Header("--- Drop Items ---")]
    [SerializeField] private PoolObject TestItem;

    [Header("--- 공격 설정 ---")]
    public float damageCooldown = 1.0f; // 공격 간격
    private float lastAttackTime;       // 마지막 공격 시간

    [Header("--- 보상 설정(Test) ---")]
    [SerializeField] protected int scoreReward = 1000; // 잡았을 때 줄 점수
    [SerializeField] protected int goldReward = 500;   // 잡았을 때 줄 골드

    private int _expReward;

    protected Rigidbody2D rb2D;
    protected bool isAttacking = false; // 패턴 중 이동 정지용

    public Vector2 Position => rb2D.position; // 외부 참조용 포지션

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.gravityScale = 0f;
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb2D.sleepMode = RigidbodySleepMode2D.NeverSleep;

        CacheBaseStats();
    }

    protected virtual void Start()
    {
        // 초기 위치 동기화
        if (rb2D != null) rb2D.position = transform.position;

        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
            else
            {
                // 여전히 못 찾았다면 씬에 Player 태그가 없는 것
                Debug.LogWarning($"{gameObject.name}: 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다.");
            }
        }
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        // ✅ 풀 재사용 시 상태 초기화
        CacheBaseStats();
        hp = _baseHp;
        power = _basePower; // 필요하다면 기본값 복구 후 ApplyScaling이 다시 덮어씀

        isAttacking = false;
        lastAttackTime = 0f;

        if (rb2D == null)
            rb2D = GetComponent<Rigidbody2D>();

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        // target 재확인
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        isAttacking = false;
        lastAttackTime = 0f;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (target == null || isAttacking) return;
        MoveToTarget();
    }

    protected virtual void MoveToTarget()
    {
        StraightChase();
    }

    // 2D 전용 직선 추격 (Rigidbody 물리 이동)
    protected void StraightChase()
    {
        Vector2 myPos = rb2D.position;
        Vector2 targetPos = target.position;
        Vector2 diff = targetPos - myPos;

        if (diff.magnitude <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = diff.normalized;
        Vector2 nextPos = myPos + dir * moveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(nextPos);
    }

    // 자식 클래스(Special, Boss)에서 사용할 좌표 적용 함수
    protected void ApplyPosition(Vector2 nextPos)
    {
        rb2D.MovePosition(nextPos);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0)
        {
            Die();
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        // 닿은 대상이 target인지 확인
        if (target != null && other.gameObject == target.gameObject)
        {
            // 공격 주기가 되었는지 확인
            if (Time.time >= lastAttackTime + damageCooldown)
            {
                // 플레이어에게 데미지 전달 시도
                IDamageable player = other.gameObject.GetComponent<IDamageable>();

                if (player != null)
                {
                    player.TakeDamage(this.power); // 플레이어의 함수 호출
                    lastAttackTime = Time.time;    // 쿨타임 초기화

                    // 확인을 위한 로그
                    //Debug.Log($"{gameObject.name}가 트리거로 플레이어에게 데미지를 입혔음.");
                }
            }
        }
    }

    public void SetExpReward(int exp)
    {
        _expReward = exp;
    }

    private void CacheBaseStats()
    {
        if (_baseCached) return;
        _baseCached = true;
        _baseHp = hp;
        _basePower = power;
    }

    public void ApplyScaling(float hpScale, float powerScale)
    {
        CacheBaseStats();
        hp = _baseHp * hpScale;
        power = _basePower * powerScale;
    }

    protected virtual void Die()
    {
        PlayerRewardHandler handler = Object.FindFirstObjectByType<PlayerRewardHandler>();

        // 몬스터를 잡으면 획득 가능한 골드를 획득 가능
        if (handler != null)
        {
            handler.AddReward(scoreReward, goldReward);
        }

        // 드랍 아이템 로직
        if (TestItem != null && ObjectPoolManager.Instance != null)
        {
            var itemObj = ObjectPoolManager.Instance.Spawn(TestItem, transform.position, Quaternion.identity);

            // Spawn이 반환하는 타입이 PoolObject라고 가정
            if (itemObj != null && itemObj.TryGetComponent<InGameExpItem>(out var expItem))
            {
                expItem.SetExp(_expReward);
            }
            else
            {
                Debug.LogWarning("[EXP] Drop spawned but InGameExpItem missing.");
            }
        }

        // 3. 반납 로직 (에러 발생 지점)
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(this);
        }
        else
        {
            // 매니저가 없으면 그냥 파괴
            Destroy(gameObject);
        }
    }
}