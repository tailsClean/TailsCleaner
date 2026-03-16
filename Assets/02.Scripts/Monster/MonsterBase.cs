using UnityEngine;
using MonsterEnum;
using System.Collections.Generic; 
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : PoolObject, IDamageable, IMonsterStatus, IPullable
{
    [Header("--- 환경 설정 ---")]
    public Transform target;
    public float stoppingDistance = 0.1f;

    [Header("--- 몬스터 정체성 ---")]
    public abstract MonsterEnum.MONSTERTYPE monsterType { get; }

    [Header("--- 기준 스탯 ---")]
    public float hp = 1.0f;
    public float maxHp = 1.0f;
    public float power = 1.0f;
    public float moveSpeed = 1.0f;
    public float hitBox = 1.0f;
    public float mass = 1.0f;
    public float KBResist = 1.0f;

    // --- IMonsterStatus 상태 프로퍼티 ---
    public bool IsWeakened { get; protected set; }
    public bool IsStunned { get; protected set; }
    public bool IsKnockbacked { get; protected set; }
    public bool HasReducedMaxHp { get; protected set; }
    public float StunAreaTime { get; protected set; }

    // --- 내부 계산용 변수 ---
    private bool _baseCached;
    private float _baseHp;
    private float _basePower;
    private float _baseMoveSpeed;
    private float _currentMoveSpeed;

    // 슬로우 중첩 관리를 위한 딕셔너리
    private Dictionary<string, float> _slowModifiers = new Dictionary<string, float>();

    [Header("--- Drop Items ---")]
    [SerializeField] private PoolObject TestItem;

    [Header("--- 공격 설정 ---")]
    public float damageCooldown = 1.0f;
    private float lastAttackTime;

    [Header("--- 보상 설정(Test) ---")]
    [SerializeField] protected int scoreReward = 1000;
    [SerializeField] protected int goldReward = 500;

    private int _expReward;

    protected Rigidbody2D rb2D;
    protected bool isAttacking = false;

    public Vector2 Position => rb2D.position;

    public int MonsterId { get; private set; } = -1;

    public void SetMonsterId(int id)
    {
        MonsterId = id;
    }

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
        if (rb2D != null) rb2D.position = transform.position;
        FindTarget();
    }

    private void FindTarget()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        CacheBaseStats();

        // 상태 초기화
        hp = _baseHp;
        maxHp = _baseHp;
        power = _basePower;
        _currentMoveSpeed = _baseMoveSpeed;

        IsWeakened = false;
        IsStunned = false;
        IsKnockbacked = false;
        HasReducedMaxHp = false;
        StunAreaTime = 0f;
        _slowModifiers.Clear();

        isAttacking = false;
        lastAttackTime = 0f;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        FindTarget();

        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.RegisterMonster(this);
        }
    }

    public override void OnDespawn()
    {
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.UnregisterMonster(this);
        }
        base.OnDespawn();
    }

    protected virtual void FixedUpdate()
    {
        // 기절이나 넉백 중에는 이동하지 않음
        if (target == null || isAttacking || IsStunned || IsKnockbacked)
        {
            if (rb2D.bodyType == RigidbodyType2D.Kinematic) rb2D.linearVelocity = Vector2.zero;
            return;
        }
        MoveToTarget();
    }

    protected virtual void MoveToTarget() => StraightChase();

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
        // _currentMoveSpeed(슬로우 적용값) 사용
        Vector2 nextPos = myPos + dir * _currentMoveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(nextPos);
    }

    // ==========================================================
    // [IMonsterStatus & IPullable 인터페이스 실제 구현]
    // ==========================================================

    public void ApplySlow(string key, float amount, float duration = -1f)
    {
        _slowModifiers[key] = amount;
        UpdateSpeed();
        if (duration > 0) StartCoroutine(RemoveSlowRoutine(key, duration));
    }

    public void RemoveSlow(string key)
    {
        if (_slowModifiers.Remove(key)) UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        float totalSlow = 0;
        foreach (var val in _slowModifiers.Values) totalSlow += val;


        if (totalSlow < 0) _currentMoveSpeed = 0;
        else _currentMoveSpeed = _baseMoveSpeed * Mathf.Max(0, (1f - totalSlow));
    }

    private IEnumerator RemoveSlowRoutine(string key, float delay)
    {
        yield return new WaitForSeconds(delay);
        RemoveSlow(key);
    }

    public void ApplyStun(float duration) => StartCoroutine(StunRoutine(duration));

    private IEnumerator StunRoutine(float duration)
    {
        IsStunned = true;
        OnCC();
        yield return new WaitForSeconds(duration);
        IsStunned = false;
    }

    public void Knockback(Vector2 direction, float force) => StartCoroutine(KnockbackRoutine(direction, force));

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        IsKnockbacked = true;
        OnCC();
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.AddForce(direction * force, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.3f); // 넉백 지속 시간
        rb2D.linearVelocity = Vector2.zero;
        rb2D.bodyType = RigidbodyType2D.Kinematic;
        IsKnockbacked = false;
    }

    public void TryReduceMaxHp(float ratio)
    {
        if (HasReducedMaxHp) return;
        maxHp *= (1f - ratio);
        if (hp > maxHp) hp = maxHp;
        HasReducedMaxHp = true;
    }

    public void OnCC() { /* SuperClean 패시브 등 연동 */ }
    public void EnterStunArea() { /* 장판 로직 */ }
    public void ExitStunArea() { /* 장판 로직 */ }
    public void ResetStunAreaTime() => StunAreaTime = 0;

    public void Pull(Vector2 targetPosition, float force)
    {
        Vector2 dir = (targetPosition - rb2D.position).normalized;
        rb2D.MovePosition(rb2D.position + dir * force * Time.fixedDeltaTime);
    }

    // ==========================================================
    // [시스템 함수]
    // ==========================================================

    public void SetExpReward(int exp)
    {
        _expReward = exp;
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
    }

    private void CacheBaseStats()
    {
        if (_baseCached) return;
        _baseCached = true;
        _baseHp = hp;
        _basePower = power;
        _baseMoveSpeed = moveSpeed;
        _currentMoveSpeed = moveSpeed;
    }

    public void ApplyScaling(float hpScale, float powerScale)
    {
        CacheBaseStats();
        hp = _baseHp * hpScale;
        maxHp = hp; // 스케일링된 체력을 최대 체력으로 설정
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

        // 반납 로직 
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

    // BossTriggerPattern때문에 작성 2026-03-16
    public virtual void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;

        if (rb2D != null && isAttacking)
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }
}