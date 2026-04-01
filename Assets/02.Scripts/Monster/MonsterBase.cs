using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : PoolObject, IDamageable, IMonsterStatus, IPullable
{
    [Header("--- 환경 설정 ---")]
    public UnityEngine.Transform target;
    public float stoppingDistance = 0.1f;

    [Header("--- 몬스터 정체성 ---")]
    public abstract MONSTERTYPE monsterType { get; }

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
    [SerializeField] protected float avoidanceRadius = 0.5f;
    [Tooltip("몬스터끼리 서로 밀어내는 힘의 세기")]
    [SerializeField] protected float avoidanceForce = 1.5f;
    [SerializeField] protected LayerMask monsterLayer;

    private float originHp;
    private float originPower;

    private float OriginHp => originHp;
    private float OriginPower => originPower;

    public float MaxHp => maxHp;

    public bool IsStunned => Time.time < _currentStunEndTime;
    public bool IsWeakened => _slowModifiers.Count > 0;
    public bool IsKnockbacked { get; protected set; }
    public bool HasReducedMaxHp { get; protected set; }
    public float StunAreaTime { get; protected set; }

    private bool _baseCached;
    private float _baseHp;
    private float _basePower;
    private float _baseMoveSpeed;
    private float _currentMoveSpeed;

    private int _stunAreaCount;
    private float _requiredStunTime;
    private float _areaStunDuration;
    private float _currentStunEndTime;

    private Dictionary<string, float> _slowModifiers = new();
    private Dictionary<string, Coroutine> _slowTimers = new();
    private Dictionary<string, int> _slowAreaCounts = new();

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

    [Header("--- 리소스 관련 ---")]
    [SerializeField] protected Animator _animator;
    protected AsyncOperationHandle<Sprite>? _spriteHandle;

    protected AsyncOperationHandle<AnimationClip>? _moveClipHandle;
    protected AsyncOperationHandle<AnimationClip>? _castClipHandle;
    protected AsyncOperationHandle<AnimationClip>? _attackClipHandle;
    protected AsyncOperationHandle<AnimationClip>? _deathClipHandle;

    protected AnimatorOverrideController _overrideController;
    protected RuntimeAnimatorController _baseAnimatorController;

    protected MonsterResource currentResourceData;
    protected string moveAnimationName;
    protected string castAnimationName;
    protected string attackAnimationName;
    protected string deathAnimationName;
    protected string attackEffectName;
    

    [Header("--- 넉백 설정 ---")]
    [SerializeField] float _knockbackDuration = 0.1f;
    [SerializeField] LayerMask _wallLayerMask;
    [SerializeField] AnimationCurve _knockbackCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    private float _knockbackUnitToPx = 1f;
    private Coroutine _knockbackCoroutine;
    private Camera _mainCamera;
    private static readonly WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    public void SetMonsterId(int id)
    {
        MonsterId = id;
        //Debug.Log($"[{name}] SetMonsterId 호출 / MonsterId:{MonsterId}");
        TryApplyMonsterResource();
    }

    private void TryApplyMonsterResource()
    {
        //Debug.Log($"[{name}] TryApplyMonsterResource 호출 / MonsterId:{MonsterId}");

        if (MonsterId <= 0)
        {
            Debug.LogWarning($"[{name}] TryApplyMonsterResource 실패: MonsterId invalid = {MonsterId}");
            return;
        }

        MonsterSO monsterSO = DataManager.Instance.GetSOData<MonsterSO>();
        if (monsterSO == null)
        {
            Debug.LogError($"[{name}] MonsterSO를 찾을 수 없습니다.");
            return;
        }

        Monster monsterData = monsterSO.GetById(MonsterId);
        if (monsterData == null)
        {
            Debug.LogError($"[{name}] 몬스터 데이터 없음. MonsterId:{MonsterId}");
            return;
        }

        MonsterResourceSO monsterResourceSO = DataManager.Instance.GetSOData<MonsterResourceSO>();
        if (monsterResourceSO == null)
        {
            Debug.LogError($"[{name}] MonsterResourceSO를 찾을 수 없습니다.");
            return;
        }

        MonsterResource resourceData = monsterResourceSO.GetById(monsterData.resource_id);
        if (resourceData == null)
        {
            Debug.LogError($"[{name}] 몬스터 리소스 데이터 없음. resource_id:{monsterData.resource_id}");
            return;
        }

        ApplyMonsterResource(resourceData);
    }

    protected virtual string GetSpriteAddress(MonsterResource resourceData)
    {
        // 노말 몬스터는 move_animation 기준으로 스프라이트 주소를 유도
        if (!string.IsNullOrEmpty(resourceData.move_animation))
        {
            string moveAddress = resourceData.move_animation;

            // 예: monster_1-1_threadball_move -> monster_1-1_threadball
            if (moveAddress.EndsWith("_move"))
                return moveAddress.Replace("_move", "");

            return moveAddress;
        }

        // move_animation도 없으면 마지막 fallback
        return resourceData.index;
    }



    protected void ReleaseSpriteHandle()
    {
        if (_spriteHandle.HasValue && _spriteHandle.Value.IsValid())
        {
            Addressables.Release(_spriteHandle.Value);
        }

        _spriteHandle = null;
    }

    protected void ReleaseAnimationHandles()
    {
        if (_moveClipHandle.HasValue && _moveClipHandle.Value.IsValid())
            Addressables.Release(_moveClipHandle.Value);

        if (_castClipHandle.HasValue && _castClipHandle.Value.IsValid())
            Addressables.Release(_castClipHandle.Value);

        if (_attackClipHandle.HasValue && _attackClipHandle.Value.IsValid())
            Addressables.Release(_attackClipHandle.Value);

        if (_deathClipHandle.HasValue && _deathClipHandle.Value.IsValid())
            Addressables.Release(_deathClipHandle.Value);

        _moveClipHandle = null;
        _castClipHandle = null;
        _attackClipHandle = null;
        _deathClipHandle = null;
    }

    protected void LoadAndApplyAnimationClip(
        string clipAddress,
        string overrideKey,
        System.Action<AsyncOperationHandle<AnimationClip>?> handleStore)
    {
        if (string.IsNullOrEmpty(clipAddress))
            return;

        //Debug.Log($"[{name}] AnimationClip load request: {clipAddress}");

        int requestedResourceId = currentResourceData != null ? currentResourceData.resource_id : -1;

        var handle = Addressables.LoadAssetAsync<AnimationClip>(clipAddress);
        handleStore(handle);

        handle.Completed += op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{name}] AnimationClip load failed: {clipAddress}");
                return;
            }

            if (currentResourceData == null || currentResourceData.resource_id != requestedResourceId)
            {
                //Debug.Log($"[{name}] AnimationClip load completed but resource changed. Skip apply: {clipAddress}");
                return;
            }

            if (_overrideController == null)
            {
                Debug.LogWarning($"[{name}] overrideController is null");
                return;
            }

            _overrideController[overrideKey] = op.Result;
            // Debug.Log($"[{name}] AnimationClip apply success: {overrideKey} -> {clipAddress}");

            if (_animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
        };
    }

    protected virtual void ResetResourceState()
    {
        currentResourceData = null;
        moveAnimationName = null;
        castAnimationName = null;
        attackAnimationName = null;
        deathAnimationName = null;
        attackEffectName = null;
    }

    public virtual void ApplyMonsterResource(MonsterResource resourceData)
    {
        if (resourceData == null)
        {
            Debug.LogWarning($"[{name}] ApplyMonsterResource 실패: resourceData null");
            return;
        }

        currentResourceData = resourceData;

        moveAnimationName = resourceData.move_animation;
        castAnimationName = resourceData.cast_animation;
        attackAnimationName = resourceData.attack_animation;
        deathAnimationName = resourceData.death_animation;
        attackEffectName = resourceData.attack_effect;

        ApplyAnimatorResource(resourceData);

        // Debug.Log(
        //     $"[{name}] Resource Applied / " +
        //     $"resource_id:{resourceData.resource_id}, " +
        //     $"index:{resourceData.index}, " +
        //     $"cast:{castAnimationName}, move:{moveAnimationName}, attack:{attackAnimationName}, death:{deathAnimationName}"
        // );
    }

    protected virtual void ApplySprite(MonsterResource resourceData)
    {
        if (_monsterSprite == null)
        {
            Debug.LogWarning($"[{name}] _monsterSprite is null");
            return;
        }

        string spriteAddress = GetSpriteAddress(resourceData);

        if (string.IsNullOrEmpty(spriteAddress))
        {
            Debug.LogWarning($"[{name}] spriteAddress is null or empty / resource_id:{resourceData.resource_id}");
            return;
        }

        ReleaseSpriteHandle();

        int requestedResourceId = resourceData.resource_id;
        string requestedAddress = spriteAddress;

        var handle = Addressables.LoadAssetAsync<Sprite>(requestedAddress);
        _spriteHandle = handle;

        handle.Completed += op =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[{name}] Addressables Sprite Load Failed: {requestedAddress}");
                return;
            }

            if (currentResourceData == null || currentResourceData.resource_id != requestedResourceId)
            {
                Debug.Log($"[{name}] Sprite load completed but resource changed. Skip apply: {requestedAddress}");
                return;
            }

            _monsterSprite.sprite = op.Result;
            Debug.Log($"[{name}] Addressables Sprite Apply Success: {requestedAddress}");
        };
    }

    protected virtual void ApplyAnimatorResource(MonsterResource resourceData)
    {
        //Debug.Log($"[ANIM CHECK] monsterId:{MonsterId}, resourceId:{resourceData.resource_id}, move_animation:{resourceData.move_animation}");

        if (_animator == null || _baseAnimatorController == null)
            return;

        ReleaseAnimationHandles();

        _overrideController = new AnimatorOverrideController(_baseAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

        // 노말 기준: move만
        if (!string.IsNullOrEmpty(resourceData.move_animation))
        {
            //Debug.Log($"[체크] 로드 시도 주소: {resourceData.move_animation}");

            LoadAndApplyAnimationClip(resourceData.move_animation, "Move_Base",
                clipHandle => _moveClipHandle = clipHandle);
        }

        _animator.Rebind();
        _animator.Update(0f);
    }

    protected virtual void ClearMonsterResource()
    {
        ReleaseSpriteHandle();
        ReleaseAnimationHandles();

        currentResourceData = null;

        moveAnimationName = null;
        castAnimationName = null;
        attackAnimationName = null;
        deathAnimationName = null;
        attackEffectName = null;

        if (_monsterSprite != null)
        {
            _monsterSprite.sprite = null;
        }

        if (_animator != null)
        {
            _animator.runtimeAnimatorController = _baseAnimatorController;
            _animator.Rebind();
            _animator.Update(0f);
        }
    }

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        if (_monsterSprite == null)
            _monsterSprite = GetComponentInChildren<SpriteRenderer>(true);

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>(true);

        if (_animator != null)
            _baseAnimatorController = _animator.runtimeAnimatorController;

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

        hp = _baseHp;
        maxHp = _baseHp;
        power = _basePower;
        _currentMoveSpeed = _baseMoveSpeed;
        _currentStrengthBonus = 0f;

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

        if (MonsterId > 0)
        {
            TryApplyMonsterResource();
        }

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

        ClearMonsterResource();

        base.OnDespawn();
    }

    protected virtual void Update()
    {
        if (isPaused)
            return;

        if (_stunAreaCount > 0 && IsStunned == false)
        {
            StunAreaTime += Time.deltaTime;

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

        if (target == null || isAttacking || IsStunned || IsKnockbacked)
        {
            if (rb2D.bodyType == RigidbodyType2D.Kinematic)
                rb2D.linearVelocity = Vector2.zero;
            return;
        }

        MoveToTarget();
    }

    protected virtual void MoveToTarget() => StraightChase();

    protected void StraightChase()
    {
        Vector2 myPos = rb2D.position;
        if (target == null) return;

        Vector2 targetPos = target.position;
        Vector2 diff = targetPos - myPos;

        if (Mathf.Abs(diff.x) > 0.01f)
        {
            _monsterSprite.flipX = (diff.x < 0);
        }

        if (diff.magnitude <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;

            // 멈췄을 때
            if (_animator != null)
                _animator.SetBool("IsMove", false);

            return;
        }

        // 이동 중
        if (_animator != null)
            _animator.SetBool("IsMove", true);

        Vector2 chaseDir = diff.normalized;
        Vector2 separationDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(myPos, avoidanceRadius, monsterLayer);

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 avoidDiff = myPos - (Vector2)neighbor.transform.position;
            float dist = avoidDiff.magnitude;

            if (dist > 0 && dist < avoidanceRadius)
            {
                separationDir += avoidDiff.normalized / dist;
            }
        }

        Vector2 finalDir = (chaseDir + separationDir * avoidanceForce).normalized;
        Vector2 nextPos = myPos + finalDir * _currentMoveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(nextPos);
    }

    public void ApplySlow(string key, float amount, float duration)
    {
        _slowModifiers[key] = amount;
        UpdateSpeed();

        if (_slowTimers.TryGetValue(key, out var existing) && existing != null)
            StopCoroutine(existing);

        _slowTimers[key] = StartCoroutine(SlowTimerCoroutine(key, duration));
    }

    private IEnumerator SlowTimerCoroutine(string key, float duration)
    {
        yield return new WaitForSeconds(duration);

        _slowModifiers.Remove(key);
        _slowTimers.Remove(key);

        UpdateSpeed();
    }

    public void EnterSlowArea(string key, float amount)
    {
        if (_slowAreaCounts.ContainsKey(key) == false)
            _slowAreaCounts[key] = 0;

        _slowAreaCounts[key]++;
        _slowModifiers[key] = amount;
        UpdateSpeed();
    }

    public void ExitSlowArea(string key)
    {
        if (_slowAreaCounts.ContainsKey(key) == false) return;

        _slowAreaCounts[key]--;

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
        foreach (var val in _slowModifiers.Values)
            totalSlow += val;

        _currentMoveSpeed = _baseMoveSpeed * Mathf.Max(0, (1f - totalSlow));
    }

    public void ApplyStun(float duration)
    {
        float newEndTime = Time.time + duration;
        if (newEndTime > _currentStunEndTime)
        {
            _currentStunEndTime = newEndTime;
        }
        OnCC();
    }

    public virtual void Knockback(Vector2 direction, float force)
    {
        if (hp <= 0) return;
        if (force <= 0f) return;

        float distance = force * _knockbackUnitToPx;
        Vector2 startPos = rb2D.position;
        Vector2 dir = direction.normalized;
        Vector2 targetPos = startPos + dir * distance;

        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        _knockbackCoroutine = StartCoroutine(KnockbackCoroutine(startPos, targetPos, dir, distance));
    }

    private IEnumerator KnockbackCoroutine(Vector2 startPos, Vector2 targetPos, Vector2 dir, float totalDistance)
    {
        IsKnockbacked = true;
        OnCC();

        bool hasCatLaundry = TryGetCatLaundry(startPos, out var catLaundry);
        bool catLaundryTriggered = false;

        float duration = _knockbackDuration;
        KnockBackOffset(startPos, dir, totalDistance, ref targetPos, ref duration);

        yield return MoveCoroutine(startPos, targetPos, duration, (nextPos) =>
        {
            if (hasCatLaundry && !catLaundryTriggered && !IsInsideScreen(nextPos))
            {
                catLaundryTriggered = true;
                float damage = maxHp * catLaundry.OffScreenDamageRatio;
                TakeDamage(damage);
            }
        });

        IsKnockbacked = false;
        _knockbackCoroutine = null;
    }

    private IEnumerator MoveCoroutine(Vector2 startPos, Vector2 targetPos, float duration, System.Action<Vector2> onMove)
    {
        float time = 0f;

        while (time < duration)
        {
            if (isPaused)
            {
                yield return _waitForFixedUpdate;
                continue;
            }

            time += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(time / duration);
            Vector2 nextPos = Vector2.Lerp(startPos, targetPos, t);

            rb2D.MovePosition(nextPos);
            onMove?.Invoke(nextPos);

            yield return _waitForFixedUpdate;
        }

        rb2D.MovePosition(targetPos);
    }

    private void KnockBackOffset(Vector2 startPos, Vector2 dir, float totalDistance, ref Vector2 target, ref float duration)
    {
        if (totalDistance <= 0.001f) return;

        RaycastHit2D hit = Physics2D.Raycast(startPos, dir, totalDistance, _wallLayerMask);

        if (hit.collider == null) return;

        float distance = Mathf.Max(0f, hit.distance);
        float ratio = distance / totalDistance;

        target = startPos + dir * distance;
        duration = _knockbackDuration * ratio;
    }

    private bool TryGetCatLaundry(Vector2 startPos, out CatLaundryModifier catLaundry)
    {
        catLaundry = null;

        if (IsInsideScreen(startPos) == false) return false;

        return SkillManager.Instance.HasPassive(out catLaundry);
    }

    private bool IsInsideScreen(Vector2 worldPos)
    {
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
        if (SkillManager.Instance != null && SkillManager.Instance.HasPassive(out SuperCleanModifier modifier))
        {
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

    public void Pull(Vector2 targetPosition)
    {
        if (hp <= 0) return;

        Vector2 startPos = rb2D.position;
        Vector2 randomOffset = Random.insideUnitCircle * 0.05f;
        Vector2 finalTargetPos = targetPosition + randomOffset;
        Vector2 diff = finalTargetPos - startPos;

        if (diff.sqrMagnitude <= 0.001f) return;

        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        _knockbackCoroutine = StartCoroutine(PullCoroutine(startPos, finalTargetPos));
    }

    private IEnumerator PullCoroutine(Vector2 startPos, Vector2 targetPos)
    {
        IsKnockbacked = true;

        yield return MoveCoroutine(startPos, targetPos, _knockbackDuration, null);

        IsKnockbacked = false;
        _knockbackCoroutine = null;
    }

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

        if (target != null && other.gameObject == target.gameObject)
        {
            if (Time.time >= lastAttackTime + damageCooldown)
            {
                IDamageable player = other.gameObject.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(this.power);
                    lastAttackTime = Time.time;
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

        RefreshFinalStats();
        hp = maxHp;
    }

    public void ApplyEnhancement(float bonusStrength)
    {
        float oldMaxHp = maxHp;

        _currentStrengthBonus = bonusStrength;
        RefreshFinalStats();

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
        if (handler != null)
        {
            handler.AddReward(scoreReward, goldReward);
        }

        if (MonsterId > 0 && RelicDropManager.Instance != null)
        {
            RelicDropManager.Instance.TryDropRelic(MonsterId, transform.position);
        }

        if (TestItem != null && ObjectPoolManager.Instance != null)
        {
            var itemObj = ObjectPoolManager.Instance.Spawn(TestItem, transform.position, Quaternion.identity);

            if (itemObj != null && itemObj.TryGetComponent<InGameExpItem>(out var expItem))
            {
                expItem.SetExp(_expReward);
            }
            else
            {
                Debug.LogWarning("[EXP] Drop spawned but InGameExpItem missing.");
            }
        }

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

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