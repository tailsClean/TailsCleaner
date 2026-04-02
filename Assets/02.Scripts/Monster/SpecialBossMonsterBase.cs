using MonsterEnum;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpecialBossMonsterBase : MonsterBase
{
    protected static List<SpecialBossMonsterBase> activeMonsters = new List<SpecialBossMonsterBase>();

    protected enum MonsterState { MOVE, PATTERN }
    protected MonsterState currentState = MonsterState.MOVE;

    [Header("--- 데이터 테이블 연동 ---")]
    public int pattern_group_id;

    [Header("--- monster_table 연동 ---")]
    public float detect_range;

    [Header("--- pattern_table 연동 ---")]
    public float pattern_cooldown;
    public float cast_time;
    public float pattern_multiply;
    public float explosion_range;
    public float damage_multiply;
    public float zigzag_width;
    public float patternFrequency = 5.0f;
    public float type_power_multiply = 1.0f;

    [Header("--- 이동 특수 패턴 설정 ---")]
    protected Pattern currentPattern;

    [Header("--- 점프 전용 상세 설정 ---")]
    public float jump_height = 2.0f;
    public Transform visualChild;

    

    protected float patternTimer = 0f;
    protected float stateTimer = 0f;
    protected bool isWaiting = false;
    protected bool isJumping = false;
    private bool hasHitTargetInCurrentJump = false;
    private Vector2 jumpStartPos;
    private Vector2 jumpTargetPos;
    private float jumpProgress = 0f;

    protected bool isFleeingState = false;
    protected bool isWaitingFlee = false;
    private Vector2 currentFleeTargetPos;

    public bool isSuicideUnit = false;
    private bool hasExploded = false;
    private float currentCastTimer;

    private BossTriggerPatternRunner _triggerRunner;

    private bool isDataInitialized = false;
    private bool isWaitingForMonsterId = false;

    protected override void Start()
    {
        base.Start();
    }

    protected override void ApplyAnimatorResource(MonsterResource resourceData)
    {
        if (_animator == null)
        {
            Debug.LogWarning($"[{name}] _animator is null");
            return;
        }

        if (_baseAnimatorController == null)
        {
            Debug.LogWarning($"[{name}] _baseAnimatorController is null");
            return;
        }

        ReleaseAnimationHandles();

        _overrideController = new AnimatorOverrideController(_baseAnimatorController);
        _animator.runtimeAnimatorController = _overrideController;

        if (!string.IsNullOrEmpty(resourceData.cast_animation))
        {
            LoadAndApplyAnimationClip(
                resourceData.cast_animation,
                "Cast_Base",
                clipHandle => _castClipHandle = clipHandle
            );
        }

        if (!string.IsNullOrEmpty(resourceData.move_animation))
        {
            LoadAndApplyAnimationClip(
                resourceData.move_animation,
                "Move_Base",
                clipHandle => _moveClipHandle = clipHandle
            );
        }

        if (!string.IsNullOrEmpty(resourceData.attack_animation))
        {
            LoadAndApplyAnimationClip(
                resourceData.attack_animation,
                "Attack_Base",
                clipHandle => _attackClipHandle = clipHandle
            );
        }

        if (!string.IsNullOrEmpty(resourceData.death_animation))
        {
            LoadAndApplyAnimationClip(
                resourceData.death_animation,
                "Death_Base",
                clipHandle => _deathClipHandle = clipHandle
            );
        }

        _animator.Rebind();
        _animator.Update(0f);

        Debug.Log(
            $"[{name}] SpecialBoss ApplyAnimatorResource / " +
            $"cast:{resourceData.cast_animation}, " +
            $"move:{resourceData.move_animation}, " +
            $"attack:{resourceData.attack_animation}, " +
            $"death:{resourceData.death_animation}"
        );
    }

    protected override void FixedUpdate()
    {
        // 기본 상태 체크 
        if (isPaused)
        {
            if (rb2D != null) rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (!isDataInitialized)
        {
            if (MonsterId <= 0) return;
            InitializeMonsterData();
            if (!isDataInitialized) return;
        }

        // 생존 및 타겟 체크
        if (target == null || hp <= 0 || hasExploded)
        {
            if (rb2D != null) rb2D.linearVelocity = Vector2.zero;
            // 사망 시 애니메이션은 별도의 Die() 로직에서 처리하므로 여기선 리턴
            return;
        }

        // --- 애니메이션 판정을 위한 상태 변수 ---
        bool isMovingNow = false;

        // 패턴 및 이동 로직 실행
        if (isSuicideUnit)
        {
            // [자폭 유닛] 캐스팅 중일 때만 타겟을 향해 이동
            if (currentCastTimer > 0f)
            {
                currentCastTimer -= Time.fixedDeltaTime;
                MoveToTarget();
                isMovingNow = true;
            }
            else
            {
                ExecuteExplosion();
                return; // 폭발 후 로직 종료
            }
        }
        else if (isAttacking)
        {
            // [공격 중] 물리 이동 정지
            rb2D.linearVelocity = Vector2.zero;
            isMovingNow = false;
        }
        else
        {
            // [일반 및 특수 이동 패턴]
            MoveToTarget();

            // 실제 이동 중인지 판정 
            if (rb2D.linearVelocity.magnitude > 0.1f || isJumping || isFleeingState)
            {
                isMovingNow = true;
            }
        }

        // 최종 애니메이터 파라미터 제어 
        if (_animator != null)
        {
            if (isAttacking || isWaiting || isWaitingFlee || IsStunned || IsKnockbacked)
            {
                _animator.SetBool("IsMove", false);
            }
            else
            {
                _animator.SetBool("IsMove", isMovingNow);
            }
        }
    }


    public override void OnSpawn()
    {
        base.OnSpawn();

        ResetRuntimeState();

        isDataInitialized = false;
        isWaitingForMonsterId = true;

        Debug.Log($"[SpecialBossMonsterBase] OnSpawn / name:{name}, MonsterId:{MonsterId}, instanceId:{GetInstanceID()} (deferred init)");
    }

    private void InitializeMonsterData()
    {
        if (isDataInitialized)
            return;

        if (MonsterId <= 0)
        {
            Debug.LogError($"[SpecialBossMonsterBase] 유효하지 않은 MonsterId: {MonsterId}, name:{name}, instanceId:{GetInstanceID()}");
            return;
        }

        Debug.Log($"[SpecialBossMonsterBase] InitializeMonsterData / name:{name}, instanceId:{GetInstanceID()}, MonsterId:{MonsterId}");

        MonsterSO monsterSO = DataManager.Instance.GetSOData<MonsterSO>();
        if (monsterSO == null)
        {
            Debug.LogError("[SpecialBossMonsterBase] MonsterSO를 찾을 수 없습니다.");
            return;
        }

        // 실제 데이터 조회
        Monster monsterData = monsterSO.GetById(MonsterId);

        // 데이터 없음 → 원인 추적 로그
        if (monsterData == null)
        {
            Debug.LogError($"[SpecialBossMonsterBase] 몬스터 데이터 없음. MonsterId:{MonsterId}, name:{name}");

            // 디버그용: 특정 ID들 존재 여부 확인
            int[] debugIds = { 102001, 102002, 200001, 201001, 202001 };

            foreach (int id in debugIds)
            {
                Monster test = monsterSO.GetById(id);
                Debug.Log($"[MonsterSO Check] id:{id}, exists:{test != null}");
            }

            return;
        }

        // 정상 데이터 로딩
        pattern_group_id = monsterData.pattern_group_id;

        if (monsterData.monster_type == MONSTERTYPE.Elite && pattern_group_id <= 0)
        {
            Debug.Log($"[SpecialBossMonsterBase] >>> Elite fallback ENTER <<< name:{name}, MonsterId:{MonsterId}, pattern_group_id:{pattern_group_id}");

            currentPattern = null;
            isSuicideUnit = false;
            currentState = MonsterState.MOVE;
            currentCastTimer = 0f;

            MonsterShooter fallbackShooter = GetComponent<MonsterShooter>();
            if (fallbackShooter != null)
            {
                fallbackShooter.DisableShooter();
            }

            if (!activeMonsters.Contains(this))
                activeMonsters.Add(this);

            if (visualChild != null)
                visualChild.localPosition = Vector2.zero;

            if (target == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                    target = playerObj.transform;
            }

            isDataInitialized = true;
            isWaitingForMonsterId = false;

            Debug.Log($"[SpecialBossMonsterBase] >>> Elite fallback COMPLETE <<< name:{name}, MonsterId:{MonsterId}, isDataInitialized:{isDataInitialized}, targetNull:{target == null}");

            return;
        }


        if (pattern_group_id <= 0)
        {
            Debug.LogError($"[SpecialBossMonsterBase] pattern_group_id invalid. MonsterId:{MonsterId}, pattern_group_id:{pattern_group_id}");
            return;
        }

        PatternGroupCompositionSO compositionSO = DataManager.Instance.GetSOData<PatternGroupCompositionSO>();
        if (compositionSO == null)
        {
            Debug.LogError("[SpecialBossMonsterBase] PatternGroupCompositionSO를 찾을 수 없습니다.");
            return;
        }

        List<PatternGroupComposition> compositionList = compositionSO.GetAllByGroupId(pattern_group_id);

        if (compositionList == null || compositionList.Count == 0)
        {
            Debug.LogError($"[SpecialBossMonsterBase] composition 데이터 없음. pattern_group_id:{pattern_group_id}");
            return;
        }

        Debug.Log($"[Composition 개수] pattern_group_id:{pattern_group_id}, count:{compositionList.Count}");

        foreach (var comp in compositionList)
        {
            Debug.Log($"[Composition] group:{comp.pattern_group_id}, pattern:{comp.pattern_id}, priority:{comp.priority}");
        }

        PatternSO patternSO = DataManager.Instance.GetSOData<PatternSO>();
        if (patternSO == null)
        {
            Debug.LogError("[SpecialBossMonsterBase] PatternSO를 찾을 수 없습니다.");
            return;
        }

        Pattern movePattern = null;
        Pattern projectilePattern = null;

        foreach (var composition in compositionList)
        {
            Pattern pattern = patternSO.GetById(composition.pattern_id);
            if (pattern == null) continue;

            Debug.Log($"[Pattern 확인] pattern_id:{pattern.pattern_id}, type:{pattern.pattern_type}, logic:{pattern.pattern_logic_type}");

            if (pattern.pattern_type == PATTERN_TYPE.Projectile && projectilePattern == null)
            {
                projectilePattern = pattern;
            }
            else if (pattern.pattern_type == PATTERN_TYPE.Move && movePattern == null)
            {
                movePattern = pattern;
            }
        }

        MonsterShooter shooter = GetComponent<MonsterShooter>();

        // Projectile 우선
        if (projectilePattern != null)
        {
            currentPattern = null;
            ResetRuntimeState();

            isSuicideUnit = false;
            damage_multiply = projectilePattern.damage_multiply > 0f ? projectilePattern.damage_multiply : 1f;
            detect_range = projectilePattern.detect_range > 0f ? projectilePattern.detect_range : detect_range;
            cast_time = 0f;
            explosion_range = 0f;

            if (shooter != null)
            {
                shooter.ApplyProjectilePattern(projectilePattern);
            }
        }
        else
        {
            if (shooter != null)
            {
                shooter.DisableShooter();
            }

            if (movePattern != null)
            {
                currentPattern = movePattern;
                ApplyPatternData(movePattern);

                Debug.Log(
                    $"[Move 적용 완료] " +
                    $"MonsterId:{MonsterId}, PatternGroupId:{pattern_group_id}, PatternId:{movePattern.pattern_id}, Logic:{movePattern.pattern_logic_type}, " +
                    $"Cooldown:{pattern_cooldown}, Cast:{cast_time}, SpeedMul:{pattern_multiply}, Detect:{detect_range}, " +
                    $"ZigzagWidth:{zigzag_width}, JumpHeight:{jump_height}, ExplosionRange:{explosion_range}, DamageMul:{damage_multiply}"
                );
            }
            else
            {
                currentPattern = null;
                ResetRuntimeState();

                Debug.Log($"[기본 이동 적용] MonsterId:{MonsterId}, PatternGroupId:{pattern_group_id}");
            }
        }

        if (!activeMonsters.Contains(this))
            activeMonsters.Add(this);

        if (isSuicideUnit)
        {
            currentCastTimer = cast_time;
            currentState = MonsterState.PATTERN;
        }
        else
        {
            currentState = MonsterState.MOVE;
        }

        if (visualChild != null)
            visualChild.localPosition = Vector2.zero;

        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
        }

        isDataInitialized = true;
        isWaitingForMonsterId = false;

        Debug.Log($"[SpecialBossMonsterBase] >>> Elite fallback COMPLETE <<< name:{name}, MonsterId:{MonsterId}, isDataInitialized:{isDataInitialized}, targetNull:{target == null}");

        return;
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        activeMonsters.Remove(this);

        hasExploded = false;
        isJumping = false;
        isWaiting = false;
        isFleeingState = false;
        isWaitingFlee = false;
        hasHitTargetInCurrentJump = false;

        patternTimer = 0f;
        stateTimer = 0f;
        jumpProgress = 0f;
        currentCastTimer = 0f;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        StopAllCoroutines();
        CancelInvoke();

        if (visualChild != null)
            visualChild.localPosition = Vector2.zero;

        if (_triggerRunner != null)
            _triggerRunner.Unbind();

        isDataInitialized = false;
        isWaitingForMonsterId = false;

        SetMonsterId(0);
    }

    protected Vector2 GetSeparationDir(Vector2 myPos)
    {
        Vector2 separationDir = Vector2.zero;
        Collider2D[] neighbors = Physics2D.OverlapCircleAll(myPos, avoidanceRadius, monsterLayer);

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == gameObject) continue;

            Vector2 avoidDiff = myPos - (Vector2)neighbor.transform.position;
            float dist = avoidDiff.magnitude;

            if (dist > 0f && dist < avoidanceRadius)
            {
                separationDir += avoidDiff.normalized / dist;
            }
        }

        return separationDir;
    }

    protected Vector2 ApplyAvoidance(Vector2 myPos, Vector2 moveDir)
    {
        if (moveDir.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        Vector2 separationDir = GetSeparationDir(myPos);
        Vector2 finalDir = moveDir + separationDir * avoidanceForce;

        if (finalDir.sqrMagnitude <= 0.0001f)
            return moveDir.normalized;

        return finalDir.normalized;
    }

    protected override void MoveToTarget()
    {
        patternTimer += Time.fixedDeltaTime;
        stateTimer += Time.fixedDeltaTime;

        if (isSuicideUnit && !hasExploded)
        {
            float suicideSpeed = moveSpeed * pattern_multiply;
            Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;
            float dist = Vector2.Distance(target.position, rb2D.position);

            if (dist > 0.1f)
            {
                Vector2 finalDir = ApplyAvoidance(rb2D.position, dir);
                rb2D.linearVelocity = finalDir * suicideSpeed;
            }
            else
            {
                rb2D.linearVelocity = Vector2.zero;
            }

            return;
        }

        if (currentPattern == null)
        {
            StraightChase();
            return;
        }

        string logic = NormalizePatternLogic(currentPattern.pattern_logic_type);

        switch (logic)
        {
            case "StraightChase":
                StraightChase();
                break;

            case "Zigzag":
                ZigzagMove();
                break;

            case "Jump":
                JumpMove();
                break;

            case "Flee":
                FleeMove();
                break;

            default:
                Debug.LogWarning($"[SpecialBossMonsterBase] 알 수 없는 pattern_logic_type: {currentPattern.pattern_logic_type} → StraightChase fallback");
                StraightChase();
                break;
        }
    }

    protected void ZigzagMove()
    {
        Vector2 myPos = rb2D.position;
        Vector2 targetPos = (Vector2)target.position;
        Vector2 toTarget = targetPos - myPos;
        float dist = toTarget.magnitude;

        Vector2 baselineDir = (dist > 0.1f) ? toTarget.normalized : Vector2.zero;
        Vector2 sideDir = new Vector2(-baselineDir.y, baselineDir.x);

        float sideOffset = Mathf.Sin(patternTimer * patternFrequency) * zigzag_width;
        float damping = Mathf.Clamp01((dist - 0.2f) / 0.8f);

        float zigzagSpeed = moveSpeed * pattern_multiply;
        Vector2 movement = (baselineDir * zigzagSpeed) + (sideDir * sideOffset * patternFrequency * damping);

        if (dist < 0.1f)
        {
            rb2D.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 finalDir = ApplyAvoidance(myPos, movement.normalized);
            rb2D.linearVelocity = finalDir * zigzagSpeed;
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSuicideUnit) return;
        if (!collision.CompareTag("Player")) return;

        IDamageable player = collision.GetComponent<IDamageable>();
        if (player == null) return;

        if (currentPattern == null)
        {
            player.TakeDamage(power);
            return;
        }

        string logic = NormalizePatternLogic(currentPattern.pattern_logic_type);

        switch (logic)
        {
            case "Zigzag":
                player.TakeDamage(power);
                break;

            case "Jump":
                if (isJumping && !hasHitTargetInCurrentJump)
                {
                    float finalJumpDamage = power * damage_multiply;
                    player.TakeDamage(finalJumpDamage);
                    hasHitTargetInCurrentJump = true;
                }
                break;

            default:
                player.TakeDamage(power);
                break;
        }
    }

    protected void JumpMove()
    {
        float distance = Vector2.Distance(target.position, rb2D.position);

        if (!isJumping && !isWaiting)
        {
            if (distance > detect_range || stateTimer < pattern_cooldown)
            {
                StraightChase();
                return;
            }

            currentState = MonsterState.PATTERN;
            isWaiting = true;
            stateTimer = 0f;
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (isWaiting)
        {
            rb2D.linearVelocity = Vector2.zero;

            if (stateTimer >= cast_time)
            {
                isWaiting = false;
                isJumping = true;
                hasHitTargetInCurrentJump = false;
                jumpStartPos = rb2D.position;
                jumpTargetPos = target.position;
                jumpProgress = 0f;
                stateTimer = 0f;
            }

            return;
        }

        if (isJumping)
        {
            float actualSpeed = moveSpeed * pattern_multiply;
            float totalDistance = Vector2.Distance(jumpStartPos, jumpTargetPos);
            float duration = (totalDistance > 0f && actualSpeed > 0f) ? totalDistance / actualSpeed : 0.1f;

            jumpProgress += Time.fixedDeltaTime / duration;

            if (hp <= 0)
            {
                if (visualChild != null) visualChild.localPosition = Vector2.zero;
                rb2D.linearVelocity = Vector2.zero;
                isJumping = false;
                return;
            }

            if (jumpProgress >= 1f)
            {
                rb2D.linearVelocity = Vector2.zero;
                rb2D.position = jumpTargetPos;
                isJumping = false;
                currentState = MonsterState.MOVE;
                stateTimer = 0f;

                if (visualChild != null)
                    visualChild.localPosition = Vector2.zero;
            }
            else
            {
                Vector2 nextTargetPos = Vector2.Lerp(jumpStartPos, jumpTargetPos, jumpProgress);
                Vector2 moveDir = nextTargetPos - rb2D.position;
                rb2D.linearVelocity = moveDir / Time.fixedDeltaTime;

                if (visualChild != null)
                {
                    float currentHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jump_height;
                    visualChild.localPosition = new Vector2(0f, currentHeight);
                }
            }
        }
    }

    protected void FleeMove()
    {
        float distanceToPlayer = Vector2.Distance(target.position, rb2D.position);

        if (!isFleeingState && !isWaitingFlee)
        {
            if (distanceToPlayer > detect_range || stateTimer < pattern_cooldown)
            {
                StraightChase();
                return;
            }

            rb2D.linearVelocity = Vector2.zero;
            isWaitingFlee = true;
            stateTimer = 0f;
            return;
        }

        if (isWaitingFlee)
        {
            rb2D.linearVelocity = Vector2.zero;

            if (stateTimer >= cast_time)
            {
                isWaitingFlee = false;
                isFleeingState = true;
                stateTimer = 0f;
                currentFleeTargetPos = GetFleePosition();
            }

            return;
        }

        if (isFleeingState)
        {
            Vector2 dir = (currentFleeTargetPos - rb2D.position).normalized;
            float distToTarget = Vector2.Distance(rb2D.position, currentFleeTargetPos);

            Vector2 finalDir = ApplyAvoidance(rb2D.position, dir);
            rb2D.linearVelocity = finalDir * (moveSpeed * pattern_multiply);

            if (distToTarget < 0.5f || Vector2.Distance(target.position, currentFleeTargetPos) < 2f)
            {
                CompleteFleePattern();
            }
        }
    }

    private void CompleteFleePattern()
    {
        isFleeingState = false;
        isWaitingFlee = false;
        rb2D.linearVelocity = Vector2.zero;
        currentState = MonsterState.MOVE;
        stateTimer = 0f;
    }

    private Vector2 GetFleePosition()
    {
        if (currentPattern == null)
            return GetSmartFleePosition();

        switch (currentPattern.escape_target)
        {
            case ESCAPE_TARGET.Reverse:
                return GetReverseFleePosition();

            case ESCAPE_TARGET.Crowd:
                return GetSmartFleePosition();

            case ESCAPE_TARGET.Target_Location:
                return GetSmartFleePosition();

            default:
                return GetSmartFleePosition();
        }
    }

    private Vector2 GetReverseFleePosition()
    {
        if (target == null) return rb2D.position;

        Vector2 dir = (rb2D.position - (Vector2)target.position).normalized;
        if (dir.sqrMagnitude <= 0.0001f)
            dir = Vector2.up;

        float fleeDistance = Mathf.Max(detect_range, 3f);
        return rb2D.position + dir * fleeDistance;
    }

    private Vector2 GetSmartFleePosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return rb2D.position;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector2 camPos = (Vector2)cam.transform.position;

        Vector2[] areaCenters = new Vector2[6];
        int[] monsterCounts = new int[6];

        for (int i = 0; i < 6; i++)
        {
            float x = (i < 3) ? camPos.x - (width / 4f) : camPos.x + (width / 4f);
            float y = camPos.y + (height / 3f) * (1 - (i % 3));
            areaCenters[i] = new Vector2(x, y);
        }

        GameObject[] allMonsters = GameObject.FindGameObjectsWithTag("Monster");

        foreach (GameObject mObj in allMonsters)
        {
            if (mObj == gameObject) continue;

            float minDist = float.MaxValue;
            int closestArea = -1;

            for (int j = 0; j < 6; j++)
            {
                float d = Vector2.Distance(mObj.transform.position, areaCenters[j]);
                if (d < minDist)
                {
                    minDist = d;
                    closestArea = j;
                }
            }

            if (closestArea != -1)
                monsterCounts[closestArea]++;
        }

        int bestAreaIndex = 0;
        int maxCount = -1;
        float maxPlayerDist = -1f;

        for (int i = 0; i < 6; i++)
        {
            float distToPlayer = Vector2.Distance(areaCenters[i], target.position);

            if (monsterCounts[i] > maxCount || (monsterCounts[i] == maxCount && distToPlayer > maxPlayerDist))
            {
                maxCount = monsterCounts[i];
                maxPlayerDist = distToPlayer;
                bestAreaIndex = i;
            }
        }

        return areaCenters[bestAreaIndex];
    }

    private void ExecuteExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        float finalDamage = power * damage_multiply;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb2D.position, explosion_range);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            IDamageable player = hit.GetComponent<IDamageable>();
            if (player != null)
            {
                player.TakeDamage(finalDamage);
            }

            break;
        }

        ObjectPoolManager.Instance.ReturnObject(this);
    }

    public override void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;

        if (isAttacking && rb2D != null)
            rb2D.linearVelocity = Vector2.zero;
    }

    protected new void StraightChase()
    {
        Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;
        float distance = Vector2.Distance(target.position, rb2D.position);

        if (distance > 0.1f)
        {
            Vector2 finalDir = ApplyAvoidance(rb2D.position, dir);
            rb2D.linearVelocity = finalDir * moveSpeed;
        }
        else
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }

    private void ApplyPatternData(Pattern patternData)
    {
        pattern_cooldown = patternData.cooldown;
        cast_time = patternData.cast_time;
        damage_multiply = (patternData.damage_multiply > 0f) ? patternData.damage_multiply : 1f;
        zigzag_width = patternData.zigzag_width;
        explosion_range = patternData.explode_range;
        detect_range = patternData.detect_range;
        jump_height = (patternData.jump_height > 0f) ? patternData.jump_height : jump_height;
        pattern_multiply = ResolvePatternMultiply(patternData);

        isSuicideUnit = (patternData.pattern_type == PATTERN_TYPE.SelfDestruct);
    }

    private void ResetRuntimeState()
    {
        hasExploded = false;
        isJumping = false;
        isWaiting = false;
        isFleeingState = false;
        isWaitingFlee = false;
        hasHitTargetInCurrentJump = false;

        patternTimer = 0f;
        stateTimer = 0f;
        jumpProgress = 0f;
        currentCastTimer = 0f;

        currentPattern = null;
        isSuicideUnit = false;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }

    private float ResolvePatternMultiply(Pattern patternData)
    {
        if (patternData == null) return 1f;

        if (patternData.rush_speed > 0f)
            return patternData.rush_speed;

        if (patternData.pattern_type == PATTERN_TYPE.Move && patternData.stat_value > 0f)
            return patternData.stat_value;

        return 1f;
    }

    private string NormalizePatternLogic(string rawLogic)
    {
        if (string.IsNullOrWhiteSpace(rawLogic))
            return "StraightChase";

        string logic = rawLogic.Trim().ToLower();

        switch (logic)
        {
            case "straightchase":
            case "straight_chase":
            case "move_straight":
            case "move_straightchase":
            case "move_chase":
                return "StraightChase";

            case "zigzag":
            case "move_zigzag":
                return "Zigzag";

            case "jump":
            case "move_jump":
                return "Jump";

            case "flee":
            case "move_flee":
                return "Flee";

            default:
                return "StraightChase";
        }
    }
}