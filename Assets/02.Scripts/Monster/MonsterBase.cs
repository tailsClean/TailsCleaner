using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float MaxHp => maxHp;

    public float knockbackUnitToPx = 100f;
    private int _stunCounter = 0; // 딕셔너리 카운터 역할

    // --- IMonsterStatus 상태 프로퍼티 ---
    public bool IsStunned => Time.time < _currentStunEndTime;
    public bool IsWeakened => _slowModifiers.Count > 0;
    public bool IsKnockbacked { get; protected set; }
    public bool HasReducedMaxHp { get; protected set; }
    public float StunAreaTime { get; protected set; }

    // --- 내부 계산용 변수 ---
    private bool _baseCached;
    private float _baseHp;
    private float _basePower;
    private float _baseMoveSpeed;
    private float _currentMoveSpeed;

    private int _stunAreaCount;         // 밟고있는 기절 장판 수
    private float _requiredStunTime;    // 기절 장판 목표 체류 시간
    private float _areaStunDuration;    // 기절 장판 기절 시간
    private float _currentStunEndTime; // 기절이 끝나는 시점

    // 슬로우 중첩 관리를 위한 딕셔너리
    private Dictionary<string, float> _slowModifiers = new();    // 적용된 슬로우 수치
    private Dictionary<string, Coroutine> _slowTimers = new();   // 적용된 슬로우 타이머
    private Dictionary<string, int> _slowAreaCounts = new();     // 밟은 슬로우 장판 수

    // --- 강화 데이터 ---
    private float _currentStrengthBonus = 0f;

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


        IsKnockbacked = false;
        HasReducedMaxHp = false;
        StunAreaTime = 0f;
        foreach (var coroutine in _slowTimers.Values)
            if (coroutine != null) StopCoroutine(coroutine);
        _slowModifiers.Clear();
        _slowTimers.Clear();
        _slowAreaCounts.Clear();
        _stunCounter = 0;
        _stunAreaCount = 0;
        _currentStunEndTime = 0f;

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


    protected virtual void Update()
    {
        // 기절 장판 위에 서있고 기절 상태 아닐 때
        if (_stunAreaCount > 0 && IsStunned == false)
        {
            // 기절 장판 체류 시간 누적
            StunAreaTime += Time.deltaTime;

            // 일정 시간 넘으면 기절
            if (StunAreaTime >= _requiredStunTime)
            { 
                // 타이머 초기화 후 기절 적용
                ResetStunAreaTime();
                ApplyStun(_areaStunDuration);
            }
        }
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

    public void ApplySlow(string key, float amount, float duration)
    {
        // 슬로우 수치 적용
        _slowModifiers[key] = amount;
        UpdateSpeed();

        // 기존 타이머 있으면 중단
        if (_slowTimers.TryGetValue(key, out var existing) && existing != null)
            StopCoroutine(existing);

        // 타이머 갱신
        _slowTimers[key] = StartCoroutine(SlowTimerCoroutine(key, duration));
    }

    private IEnumerator SlowTimerCoroutine(string key, float duration)
    {
        // 슬로우 유지시간 대기
        yield return new WaitForSeconds(duration);

        // 적용 수치랑 타이머 제거
        _slowModifiers.Remove(key);
        _slowTimers.Remove(key);

        UpdateSpeed();
    }

    public void EnterSlowArea(string key, float amount)
    {
        if (_slowAreaCounts.ContainsKey(key) == false)
            _slowAreaCounts[key] = 0;

        // 장판 카운트
        _slowAreaCounts[key]++;

        _slowModifiers[key] = amount; // 같은 장판이면 수치 동일
        UpdateSpeed();
    }
    public void ExitSlowArea(string key)
    {
        if (_slowAreaCounts.ContainsKey(key) == false) return;

        // 장판 카운트 빼기
        _slowAreaCounts[key]--;

        // 카운트 0 될 때만 슬로우 제거
        if (_slowAreaCounts[key] <= 0)
        {
            _slowAreaCounts.Remove(key);
            _slowModifiers.Remove(key);
            UpdateSpeed();
        }
    }

    private void UpdateSpeed()
    {
        float totalSlow = 0f;
        foreach (var val in _slowModifiers.Values) totalSlow += val;

        _currentMoveSpeed = _baseMoveSpeed * Mathf.Max(0, (1f - totalSlow));
    }

    // [기절 관리]
    public void ApplyStun(float duration)
    {
        float newEndTime = Time.time + duration;
        // 기존 기절 시간보다 길 경우에만 갱신 (강제 기절 포함)
        if (newEndTime > _currentStunEndTime)
        {
            _currentStunEndTime = newEndTime;
        }
        OnCC();
    }

    public void Knockback(Vector2 direction, float force)
        => StartCoroutine(KnockbackRoutine(direction, force));

    private IEnumerator KnockbackRoutine(Vector2 direction, float force)
    {
        IsKnockbacked = true;
        OnCC();
        rb2D.bodyType = RigidbodyType2D.Dynamic;
        rb2D.AddForce(direction.normalized * force * knockbackUnitToPx, ForceMode2D.Impulse);
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

    public void OnCC()
    {
        // 집중공략 패시브
        // 군중제어 적용 시 추가로 일정시간 슬로우 적용
        if (SkillManager.Instance != null && SkillManager.Instance.HasPassive<SuperCleanModifier>(out var modifier))
        {
            ApplySlow(SuperCleanModifier.DEBUFF_KEY, modifier.SlowAmont, modifier.SlowDuration);
        }
    }

    public void EnterStunArea(float requireTime, float duration)
    {
        // 기절 장판 카운트
        _stunAreaCount++;
        // 기절에 필요한 시간
        _requiredStunTime = requireTime;
        // 기절 시간
        _areaStunDuration = duration;
    }

    public void ExitStunArea()
    {
        // 기절 장판 카운트 빼기
        _stunAreaCount--;
        // 장판 카운트 0 밑으로 떨어지면 기절 시간 초기화
        if (_stunAreaCount <= 0)
        {
            _stunAreaCount = 0;
            ResetStunAreaTime();
        }
    }

    // 기절 시간 초기화
    public void ResetStunAreaTime() => StunAreaTime = 0;

    // 당기기 (물폭탄 소용돌이)
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
        _baseHp = hp * hpScale;
        _basePower = power * powerScale;

        // 강화 수치가 이미 있다면 적용
        RefreshFinalStats();

        hp = maxHp; // 스폰 시점 기준
    }

    public void ApplyEnhancement(float bonusStrength)
    {
        // 기존 적들은 증가량만큼 현재 체력도 늘려줌
        float oldMaxHp = maxHp;

        _currentStrengthBonus = bonusStrength;

        RefreshFinalStats();

        // 현재 체력 보정 (최대 체력이 늘어난 만큼 현재 체력도 더해줌)
        float hpDiff = maxHp - oldMaxHp;
        if (hpDiff > 0) hp += hpDiff;
    }

    private void RefreshFinalStats()
    {
        float strengthBonus = 1f + _currentStrengthBonus / 100f;
        maxHp = _baseHp * strengthBonus;
        power = _basePower * strengthBonus;
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