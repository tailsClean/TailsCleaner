using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : PoolObject, IDamageable, IMonsterStatus, IPullable
{
    [Header("--- 환경 설정 ---")]
    public UnityEngine.Transform target;
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
    public float knockbackUnitToPx = 100f;

    [Header("--- 겹침 방지 설정 ---")]
    [Tooltip("몬스터끼리 서로 밀어내기 시작하는 거리")]
    [SerializeField] private float avoidanceRadius = 0.5f; // 몬스터끼리 띄울 거리
    [Tooltip("몬스터끼리 서로 밀어내는 힘의 세기")]
    [SerializeField] private float avoidanceForce = 1.5f;  // 밀어내는 힘의 세기
    [SerializeField] private LayerMask monsterLayer;       // 몬스터 전용 레이어 

    private float originHp;
    private float originPower;

    private float OriginHp => originHp;
    private float OriginPower => originPower;

    public float MaxHp => maxHp;

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
    private float _currentStunEndTime;  // 기절이 끝나는 시점

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

    [Header("---몬스터 스프라이트---")]
    [SerializeField] public SpriteRenderer _monsterSprite;
    
    [Header("--- 넉백 설정 ---")]
    [SerializeField] float _knockbackDuration = 0.1f;                                           // 넉백 시간
    [SerializeField] LayerMask _wallLayerMask;                                                  // 벽 레이어
    [SerializeField] AnimationCurve _knockbackCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);    // 선형은 Linear, 아니면 EaseInOut
    private float _knockbackUnitToPx = 1f;                                                      // 넉백 픽셀
    private Coroutine _knockbackCoroutine;                                                      // 넉백 코루틴
    private Camera _mainCamera;                                                                 // 카메라
    private static readonly WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();  // 대기시간


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
        originHp = hp;
        originPower = power;

        _mainCamera = Camera.main;

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

        // 상태 초기화
        hp = _baseHp;
        maxHp = _baseHp;
        power = _basePower;
        _currentMoveSpeed = _baseMoveSpeed;
        _currentStrengthBonus = 0f;
        Debug.Log($"hp{hp}");


        IsKnockbacked = false;
        HasReducedMaxHp = false;
        StunAreaTime = 0f;
        _currentStunEndTime = 0f;
        _stunAreaCount = 0;
        _slowModifiers.Clear();
        _slowTimers.Clear();
        _slowAreaCounts.Clear();

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
        if (isPaused)
            return;

        // 기절 장판 위에 서있고 기절 상태 아닐 때
        if (_stunAreaCount > 0 && IsStunned == false)
        {
            // 기절 장판 체류 시간 누적
            StunAreaTime += Time.deltaTime;

            // 일정 시간 넘으면 기절 후 초기화
            if (StunAreaTime >= _requiredStunTime)
            {
                ResetStunAreaTime();
                ApplyStun(_areaStunDuration);
            }
        }

    }

    protected virtual void FixedUpdate()
    {
        if (isPaused)
        {
            if (rb2D != null)
                rb2D.linearVelocity = Vector2.zero;
            return;
        }

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
        // 기본 위치 및 타겟 확인
        Vector2 myPos = rb2D.position;
        if (target == null) return; // 타겟이 없으면 중단

        Vector2 targetPos = target.position;
        Vector2 diff = targetPos - myPos;

        // 정지 거리 체크 (도착 시 멈춤)
        if (diff.magnitude <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        //  플레이어를 향한 기본 방향 벡터
        Vector2 chaseDir = diff.normalized;

        // 주변 몬스터를 피하는 회피 벡터 계산
        Vector2 separationDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(myPos, avoidanceRadius, monsterLayer);

        foreach (var neighbor in neighbors)
        {
            // 자기 자신 제외
            if (neighbor.gameObject == gameObject) continue;

            Vector2 avoidDiff = myPos - (Vector2)neighbor.transform.position;
            float dist = avoidDiff.magnitude;

            if (dist > 0 && dist < avoidanceRadius)
            {
                // 거리가 가까울수록 더 강하게 밀어내도록 합산
                separationDir += avoidDiff.normalized / dist;
            }
        }

        // 두 벡터를 합쳐서 최종 방향 결정
        // avoidanceForce를 통해 거리 조절.
        Vector2 finalDir = (chaseDir + separationDir * avoidanceForce).normalized;

        // 리지드바디 이동 적용
        Vector2 nextPos = myPos + finalDir * _currentMoveSpeed * Time.fixedDeltaTime;
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
    { 
        // 죽은 상태면 패스
        if (hp <= 0) return;
        // 넉백 힘 없으면 패스
        if (force <= 0f) return;

        float distance = force * _knockbackUnitToPx;    // 넉백 거리
        Vector2 startPos = rb2D.position;              // 넉백 시작 위치
        Vector2 dir = direction.normalized;       // 넉백 방향
        Vector2 targetPos = startPos + dir * distance;  // 넉백 목표 위치

        // 넉백 중이면 중단
        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        // 넉백 실행
        _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(startPos, targetPos, dir, distance));
    }

    private IEnumerator KnockbackCoroutine(Vector2 startPos, Vector2 targetPos, Vector2 dir, float totalDistance)
    {
        IsKnockbacked = true;
        OnCC();

        float pauseWaitElapsed = 0f;
        while (pauseWaitElapsed < 0.3f)
        {
            if (!isPaused)
                pauseWaitElapsed += Time.deltaTime;

            yield return null;
        }

        bool hasCatLaundry = TryGetCatLaundry(startPos, out var catLaundry);
        bool catLaundryTriggered = false;

        float duration = _knockbackDuration;
        KnockBackOffset(startPos, dir, totalDistance, ref targetPos, ref duration);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (isPaused)
            {
                yield return _waitForFixedUpdate;
                continue;
            }

            elapsed += Time.fixedDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float curvedT = _knockbackCurve.Evaluate(normalizedTime);

            Vector2 nextPos = Vector2.LerpUnclamped(startPos, targetPos, curvedT);
            rb2D.MovePosition(nextPos);

            if (hasCatLaundry && !catLaundryTriggered && !IsInsideScreen(nextPos))
            {
                catLaundryTriggered = true;

                float damage = maxHp * catLaundry.OffScreenDamageRatio;
                TakeDamage(damage);
            }

            yield return _waitForFixedUpdate;
        }

        rb2D.MovePosition(targetPos);

        IsKnockbacked = false;
        _knockbackCoroutine = null;
    }

    // 넉백 벽 충돌 보정
    // 벽 있으면 목표 지점, 지속 시간을 비율로 줄임
    private void KnockBackOffset(Vector2 startPos, Vector2 dir, float totalDistance, ref Vector2 target, ref float duration)
    {
        // 너무 짧은 거리면 패스
        if (totalDistance <= 0.001f) return;

        // 레이캐스트로 벽 판정
        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, totalDistance, _wallLayerMask);

        // 벽 없으면 패스
        if (hit.collider == null) return;

        // 거리 비율 (총 넉백 거리, 벽 충돌 거리)
        float distance = Mathf.Max(0f, hit.distance);
        float ratio = distance / totalDistance;

        target = startPos + dir * distance;     // 목표 지점 갱신
        duration = _knockbackDuration * ratio;  // 넉백 시간 갱신
    }

    // 냥빨래 설정
    // 루프 전에 한 번만 패시브 체크
    // 화면 밖 시작이면 냥빨래 대상 아님
    private bool TryGetCatLaundry(Vector2 startPos, out CatLaundryModifier catLaundry)
    {
        catLaundry = null;

        if (IsInsideScreen(startPos) == false) return false;

        return SkillManager.Instance.HasPassive(out catLaundry);
    }

    // 화면 내부인지 체크
    private bool IsInsideScreen(Vector2 worldPos)
    {
        // 캐싱된 카메라 없으면 그냥 항상 화면 내부 판정
        if (_mainCamera == null) return true;

        Vector3 viewPoint = _mainCamera.WorldToViewportPoint(worldPos);
        return viewPoint.x >= 0f && viewPoint.x <= 1f &&
               viewPoint.y >= 0f && viewPoint.y <= 1f;
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
        // SuperClean
        if (SkillManager.Instance != null && SkillManager.Instance.HasPassive(out SuperCleanModifier modifier))
        {
            // 이동속도 감소
            ApplySlow(SuperCleanModifier.DEBUFF_KEY, modifier.SlowAmont, modifier.SlowDuration);
        }
    }

    public void EnterStunArea(float requireTime, float duration)
    {
        _stunAreaCount++;
        _requiredStunTime = requireTime;
        _areaStunDuration = duration;
    }

    public void ExitStunArea()
    {
        _stunAreaCount--;
        if (_stunAreaCount <= 0)
        {
            _stunAreaCount = 0;
            ResetStunAreaTime();
        }
    }

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

    public virtual void TakeDamage(float damage)
    {
        hp -= damage;

        if (hp <= 0) Die();
    }
    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        if (isPaused)
            return;

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
        _baseHp = OriginHp * hpScale;
        _basePower = OriginPower * powerScale;
        Debug.Log(OriginPower + "기본 파워" + powerScale + "파워 배율 ");

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
        maxHp = _baseHp;
        power = _basePower;
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

    protected bool isPaused;

    public virtual void SetPaused(bool paused)
    {
        isPaused = paused;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        if (paused)
        {
            isAttacking = false;
        }
    }

    public bool IsPaused => isPaused;
}