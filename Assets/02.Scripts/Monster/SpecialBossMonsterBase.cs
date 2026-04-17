using MonsterEnum;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

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
    protected float currentMoveTime;

    private Bounds cachedMapBounds;   // 맵 전체 영역 캐싱
    private bool isMapInitialized = false;

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

        UpdateFacingDirection();

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
            Debug.LogError($"[SpecialBossMonsterBase] 유효하지 않은 MonsterId: {MonsterId}, name:{name}");
            return;
        }

        MonsterSO monsterSO = DataManager.Instance.GetSOData<MonsterSO>();
        Monster monsterData = monsterSO?.GetById(MonsterId);

        if (monsterSO == null || monsterData == null)
        {
            Debug.LogError($"[SpecialBossMonsterBase] 몬스터 데이터 로드 실패. MonsterId:{MonsterId}");
            return;
        }

        // 1. 몬스터 타입 데이터 적용 (Mass 등)
        MonsterTypeSO monsterTypeSO = DataManager.Instance.GetSOData<MonsterTypeSO>();
        if (monsterTypeSO != null)
        {
            var typeData = monsterTypeSO.dataList.FirstOrDefault(x => x.monster_type == monsterData.monster_type);
            if (typeData != null)
            {
                this.mass = typeData.type_mass;
            }
        }

        pattern_group_id = monsterData.pattern_group_id;

        // 2. 패턴 그룹 데이터 가져오기
        PatternGroupCompositionSO compositionSO = DataManager.Instance.GetSOData<PatternGroupCompositionSO>();
        List<PatternGroupComposition> compositionList = compositionSO?.GetAllByGroupId(pattern_group_id);
        PatternSO patternSO = DataManager.Instance.GetSOData<PatternSO>();

        // [핵심 수정] 패턴이 1가지만 존재하므로, 첫 번째 유효한 패턴을 바로 선택
        Pattern selectedPattern = null;
        if (compositionList != null && compositionList.Count > 0)
        {
            foreach (var comp in compositionList)
            {
                selectedPattern = patternSO?.GetById(comp.pattern_id);
                if (selectedPattern != null) break;
            }
        }

        MonsterShooter shooter = GetComponent<MonsterShooter>();

        // 3. 패턴 데이터 적용 및 상태 결정
        if (selectedPattern != null)
        {
            // ApplyPatternData 내부에서 isSuicideUnit 판정(SelfDestruct 체크 등)이 이루어짐
            ApplyPatternData(selectedPattern);

            if (selectedPattern.pattern_type == PATTERN_TYPE.Projectile)
            {
                // [투사체 유닛]
                currentPattern = null;
                isSuicideUnit = false;
                if (shooter != null) shooter.ApplyProjectilePattern(selectedPattern);
                currentState = MonsterState.MOVE;
            }
            else
            {
                // [이동 또는 자폭 유닛]
                currentPattern = selectedPattern;
                if (shooter != null) shooter.DisableShooter();

                if (isSuicideUnit)
                {
                    // 자폭 유닛 전용 초기화
                    currentCastTimer = cast_time;
                    currentState = MonsterState.PATTERN;
                    Debug.Log($"[{name}] 자폭 유닛 설정 완료! 패턴ID: {selectedPattern.pattern_id}, 캐스팅: {cast_time}s");
                }
                else
                {
                    currentState = MonsterState.MOVE;
                }
            }
        }
        else
        {
            // 패턴이 없거나 엘리트 폴백 처리
            Debug.Log($"[{name}] 기본 이동 모드 적용 (패턴 없음)");
            currentPattern = null;
            isSuicideUnit = false;
            currentState = MonsterState.MOVE;
            if (shooter != null) shooter.DisableShooter();
        }

        // 4. 공통 시각 설정 및 타겟 할당
        this.transform.localScale = Vector3.one;
        if (visualChild != null)
        {
            visualChild.localPosition = Vector2.zero;
            visualChild.localScale = new Vector3(this.mass, this.mass, 1f);
        }

        if (!activeMonsters.Contains(this))
            activeMonsters.Add(this);

        if (target == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) target = playerObj.transform;
        }

        isDataInitialized = true;
        isWaitingForMonsterId = false;
    }

    private void InitializeMapBounds()
    {
        Tilemap[] allTilemaps = GameObject.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        if (allTilemaps.Length == 0) return;

        bool firstFound = false;
        foreach (var tm in allTilemaps)
        {
            // 이름에 "Tower_"가 포함된 타일맵들만 합산
            if (tm.name.Contains("Tower_"))
            {
                // 월드 좌표 기준의 Bounds 계산
                Bounds worldBounds = new Bounds(tm.transform.TransformPoint(tm.localBounds.center), tm.localBounds.size);

                if (!firstFound)
                {
                    cachedMapBounds = worldBounds;
                    firstFound = true;
                }
                else
                {
                    cachedMapBounds.Encapsulate(worldBounds);
                }
            }
        }
        isMapInitialized = firstFound;
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

    private void UpdateFacingDirection()
    {
        if (target == null || visualChild == null) return;

        float lookX;
        if (isFleeingState)
            lookX = rb2D.linearVelocity.x;
        else
            lookX = target.position.x - transform.position.x;

        if (Mathf.Abs(lookX) > 0.01f)
        {
            float finalScaleX = (lookX > 0) ? this.mass : -this.mass;

            visualChild.localScale = new Vector3(finalScaleX, this.mass, 0f);
        }
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

        // [1] 상태 진입: 점프 준비
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

        // [2] 대기 중 (캐스팅 타임)
        if (isWaiting)
        {
            rb2D.linearVelocity = Vector2.zero;

            if (stateTimer >= cast_time)
            {
                isWaiting = false;
                isJumping = true;
                hasHitTargetInCurrentJump = false;
                jumpProgress = 0f;
                stateTimer = 0f;

                // --- Raycast 위치 보정 Logic 시작 ---
                jumpStartPos = rb2D.position;
                Vector2 rawTargetPos = (Vector2)target.position;
                Vector2 direction = (rawTargetPos - jumpStartPos).normalized;
                float targetDistance = Vector2.Distance(jumpStartPos, rawTargetPos);
            
                int wallMask = LayerMask.GetMask("Wall");
                RaycastHit2D hit = Physics2D.Raycast(jumpStartPos, direction, targetDistance, wallMask);

                if (hit.collider != null)
                {
                    // 벽에 걸렸다면 벽 지점에서 약간(0.5f) 앞에서 멈추도록 설정
                    jumpTargetPos = hit.point - (direction * 0.5f);

                    //Debug.DrawLine(jumpStartPos, hit.point, Color.red, 1.0f);
                }
                else
                {
                    // 경로에 벽이 없다면 정상적으로 타겟 위치까지 점프
                    jumpTargetPos = rawTargetPos;
                }
                // --- Logic 끝 ---
            }
            return;
        }

        // 점프 중 (이동 실행)
        if (isJumping)
        {
            // 사망 체크
            if (hp <= 0)
            {
                if (visualChild != null) visualChild.localPosition = Vector2.zero;
                rb2D.linearVelocity = Vector2.zero;
                isJumping = false;
                return;
            }

            float actualSpeed = moveSpeed * pattern_multiply;
            float totalDistance = Vector2.Distance(jumpStartPos, jumpTargetPos);

            // 거리나 속도가 0일 때의 예외 처리
            float duration = (totalDistance > 0.01f && actualSpeed > 0f) ? totalDistance / actualSpeed : 0.1f;

            jumpProgress += Time.fixedDeltaTime / duration;

            if (jumpProgress >= 1f)
            {
                // [착지 완료]
                rb2D.linearVelocity = Vector2.zero;
                rb2D.position = jumpTargetPos;
                isJumping = false;
                currentState = MonsterState.MOVE;
                stateTimer = 0f;

                if (GetComponent<Collider2D>() != null)
                    GetComponent<Collider2D>().isTrigger = false;

                if (visualChild != null)
                    visualChild.localPosition = Vector2.zero;
            }
            else
            {
                // [공중 이동]
                Vector2 nextTargetPos = Vector2.Lerp(jumpStartPos, jumpTargetPos, jumpProgress);
                Vector2 moveDir = nextTargetPos - rb2D.position;

                // 물리 엔진과 충돌을 최소화하기 위해 Velocity를 계산하여 적용
                rb2D.linearVelocity = moveDir / Time.fixedDeltaTime;

                // 시각적인 높이 표현 (포물선)
                if (visualChild != null)
                {
                    float currentHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jump_height;
                    visualChild.localPosition = new Vector2(0f, currentHeight);
                }
            }
        }
    }

    private float fleeTargetTimer = 0f; // 타겟 재탐색 타이머

    protected void FleeMove()
    {
        if (currentPattern == null) { StraightChase(); return; }

        float distToPlayer = Vector2.Distance(rb2D.position, target.position);

        // 1. 도망 상태 진입 및 유지 로직 (핵심 수정)
        if (!isFleeingState)
        {
            // 도망 범위 안에 들어왔을 때만 시작
            if (distToPlayer <= detect_range)
            {
                isFleeingState = true;
                stateTimer = 0f; // 도망 시작 시간 초기화
                currentFleeTargetPos = GetFleePosition();
                fleeTargetTimer = 0f;
            }
        }
        else
        {
            // [중요] 도망 중일 때는 stateTimer가 currentMoveTime(예: 6초)을 다 채울 때까지 
            // 플레이어와의 거리와 상관없이 isFleeingState를 true로 유지합니다.
            if (stateTimer >= currentMoveTime)
            {
                // 시간이 다 됐고, 플레이어와도 충분히 멀어졌다면 도망 종료
                if (distToPlayer > detect_range * 1.2f)
                {
                    isFleeingState = false;
                    stateTimer = 0f;
                }
            }
        }

        // 도망 상태가 아니면 평소처럼 추격
        if (!isFleeingState)
        {
            StraightChase();
            return;
        }

        // 2. 타겟 캐싱 및 주기적 갱신
        fleeTargetTimer += Time.fixedDeltaTime;
        if (fleeTargetTimer >= 0.5f) // 0.5초마다 더 "스마트"한 위치 재계산
        {
            currentFleeTargetPos = GetFleePosition();
            fleeTargetTimer = 0f;
        }

        // 3. 이동 실행
        Vector2 myPos = rb2D.position;
        float distToTarget = Vector2.Distance(currentFleeTargetPos, myPos);

        // 목표 지점(동료 무리)에 너무 가까우면 미세하게 떨지 않도록 정지
        if (distToTarget < 0.5f)
        {
            rb2D.linearVelocity = Vector2.zero;
        }
        else
        {
            Vector2 dir = (currentFleeTargetPos - myPos).normalized;
            float fleeSpeed = moveSpeed * pattern_multiply;

            // 장애물/동료 회피를 적용하여 최종 이동
            Vector2 finalDir = ApplyAvoidance(myPos, dir);
            rb2D.linearVelocity = finalDir * fleeSpeed;
        }
    }

    private Vector2 GetFleePosition()
    {
        return GetSmartFleePosition();
    }

    //2
    private Vector2 GetSmartFleePosition()
    {
        if (target == null) return rb2D.position;

        // 1. 현재 화면(카메라)의 월드 좌표 범위 가져오기
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector2 camPos = cam.transform.position;

        float minX = camPos.x - width / 2f;
        float minY = camPos.y - height / 2f;
        float sectorWidth = width / 3f;
        float sectorHeight = height / 2f;

        Vector2 bestTargetPos = rb2D.position;
        int maxAllyCount = -1;
        float maxDistance = -1f;

        int monsterMask = LayerMask.GetMask("Monster");

        // 2. 화면 내 6개 구역 탐색
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                Vector2 sectorCenter = new Vector2(
                    minX + (sectorWidth * x) + (sectorWidth * 0.5f),
                    minY + (sectorHeight * y) + (sectorHeight * 0.5f)
                );

                // 해당 구역 내 몬스터 수 체크
                Collider2D[] allies = Physics2D.OverlapBoxAll(sectorCenter, new Vector2(sectorWidth, sectorHeight), 0, monsterMask);

                // 본인 제외 카운트
                int currentAllyCount = 0;
                foreach (var ally in allies)
                {
                    if (ally.gameObject != this.gameObject) currentAllyCount++;
                }

                // 플레이어(target)와의 거리
                float distToPlayer = Vector2.Distance(sectorCenter, target.position);

                // 우선순위: 1. 몬스터가 많은 곳 / 2. 동률이면 플레이어와 먼 곳
                if (currentAllyCount > maxAllyCount)
                {
                    maxAllyCount = currentAllyCount;
                    maxDistance = distToPlayer;
                    bestTargetPos = sectorCenter;
                }
                else if (currentAllyCount == maxAllyCount)
                {
                    if (distToPlayer > maxDistance)
                    {
                        maxDistance = distToPlayer;
                        bestTargetPos = sectorCenter;
                    }
                }
            }
        }

        return bestTargetPos;
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
        if (patternData == null) return;

        // 기본 수치 할당
        this.currentPattern = patternData;
        pattern_cooldown = patternData.cooldown;
        damage_multiply = (patternData.damage_multiply > 0f) ? patternData.damage_multiply : 1f;
        zigzag_width = patternData.zigzag_width;
        explosion_range = patternData.explode_range;
        detect_range = patternData.detect_range;
        jump_height = (patternData.jump_height > 0f) ? patternData.jump_height : jump_height;
        pattern_multiply = ResolvePatternMultiply(patternData);

        this.currentMoveTime = (patternData.move_time > 0f) ? patternData.move_time : 6.0f;
        Debug.Log($"[ApplyPatternData] 패턴ID: {patternData.pattern_id}, 로직: {patternData.pattern_logic_type}, 도망타입: {patternData.escape_target}");

        isSuicideUnit = (patternData.pattern_type == PATTERN_TYPE.SelfDestruct ||
                         (patternData.pattern_logic_type != null && patternData.pattern_logic_type.ToLower().Contains("self")));

 
        if (patternData.duration > 0f)
        {
            this.cast_time = patternData.duration;
        }
        else if (patternData.cast_time > 0f)
        {
            this.cast_time = patternData.cast_time;
        }
        else
        {
            this.cast_time = isSuicideUnit ? 3.0f : 0f;

            if (isSuicideUnit)
                Debug.LogWarning($"[{patternData.pattern_id}] 자폭 데이터(duration/cast_time)가 0입니다. 기본값 3s를 적용합니다.");
        }

        Debug.Log($"[ApplyPatternData] ID: {patternData.pattern_id}, Type: {patternData.pattern_type}, Final CastTime: {this.cast_time}s");
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
            case "move_escape":
                return "Flee";

            default:
                return "StraightChase";
        }
    }
}