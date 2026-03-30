using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster : MonsterBase, ILaserable
{
    public override MonsterEnum.MONSTERTYPE monsterType => MonsterEnum.MONSTERTYPE.Boss;

    [Header("--- 데이터 테이블 연동 ---")]
    public int pattern_group_id;

    [Header("--- 패턴 중첩 활성화 제어 ---")]
    public bool useZigzag;
    public bool useJump;
    public bool useFlee;
    public bool useBlink;
    public bool useBarricade;
    public bool useOrbit;
    

    private List<GameObject> activeOrbits = new List<GameObject>();
    private bool isOrbiting = false;

    [Header("--- 기획 데이터 연동 ---")]
    public float move_speed;
    public float detect_range;
    public float pattern_cooldown;
    public float cast_time;
    public float pattern_multiply;
    public float pattern_damage;
    public float jump_height = 3.0f;
    public Transform visualChild;
    public float fleeDistance = 4.0f;

    public float zigzag_width = 1.5f;
    public float zigzagFrequency = 5.0f;

    private float zigzagTimer = 0f;

    [Header("---UI 변경용 이벤트 채널---")]
    [SerializeField] private FloatEventChannelSO _onBossHit;

    [Header("--- 점멸 패턴 설정 ---")]
    public float blink_detect_range = 5.0f;
    public float blink_cast_time = 1.0f;
    public float blink_speed_multiplier = 4.0f;
    public float blink_cooldown = 3.0f;
    public LineRenderer lineRenderer;
    [SerializeField] private float blinkLineWidth = 1.0f;

    [Header("--- 바리케이드 패턴 설정 ---")]
    public BarricadeSpawner barricadeSpawner;
    public float barricadeInterval = 7.0f;
    public BarricadeSpawner.SpawnLocation spawnLoc = BarricadeSpawner.SpawnLocation.Player;
    public BarricadeSpawner.BarricadeShape barShape = BarricadeSpawner.BarricadeShape.Rectangle;
    public Vector2 barSize = new Vector2(3f, 1f);
    public float barDuration = 5.0f;
    public BarricadeSpawner.InteractionType barInteraction = BarricadeSpawner.InteractionType.BlockedWithDamage;

    [Header("--- 영역 패턴 설정 ---")]
    public ZoneSpawner zoneSpawner;
    private readonly List<BossAreaPatternRuntime> areaPatterns = new List<BossAreaPatternRuntime>();
    private bool isAreaPatternRunning = false;

    [Header("--- 레이저 패턴 설정 ---")]
    // 레이저 패턴 인터페이스
    public Transform MyTransform => transform;
    private LaserPattern _laserPattern;
    


    private class BossAreaPatternRuntime
    {
        public bool isSafeZone;
        public float cooldown;
        public float currentCooldown;
        public float previewTime;
        public float activeTime;
        public float radius;
        public float damagePerTick;
        public float damageInterval;
        public int count;
        public float range;
        public ZoneTestCaller.SpawnTarget targetType;
    }

    [Header("--- 공전 투사체 패턴 설정 ---")]
    public GameObject orbitPrefab;
    public GameObject explosionEffect;

    public int defaultOrbitCount = 5;
    public float defaultOrbitRadius = 2.0f;
    public float defaultOrbitRotateSpeed = 50f;
    public float defaultOrbitDuration = 10f;
    public float defaultOrbitSpawnInterval = 0.5f;
    public float defaultOrbitDamageMultiplier = 1.0f;
    public float defaultOrbitProjectileScale = 1.2f;

    private int orbitCount = 5;
    private float orbitRadius = 2.0f;
    private float orbitRotateSpeed = 50f;
    private float orbitDuration = 10f;
    private float orbitSpawnInterval = 0.5f;
    private float orbitDamageMultiplier = 1.0f;
    private float orbitProjectileScale = 0.6f;
    private float orbitPatternCooldown = 3f;
    public float orbitRotateSpeedMultiplier = 30f;


    // 내부 제어 변수
    private float jumpCooldownTimer = 0f;
    private float fleeCooldownTimer = 0f;
    private float blinkCooldownTimer = 0f;
    private float barricadeTimer = 0f;
    private float orbitCooldownTimer = 0f;

    public float damage;

    private bool isJumping = false;
    private bool isWaitingJump = false;
    private bool isFleeing = false;
    private bool isBlinking = false;
    private bool isWaitingBlink = false;
    private bool isWaitingBarricade = false; 

    private bool isDataInitialized = false;
    private bool isWaitingForMonsterId = false;

    private Vector2 currentFleeTarget;

    private BossMonsterShooter shooter;


    private float GetMinimumOrbitRadius()
    {
        float estimatedDiameter = Mathf.Max(0.1f, orbitProjectileScale);
        float circumference = estimatedDiameter * orbitCount * 1.2f;
        return circumference / (2f * Mathf.PI);
    }

    public override void ApplyMonsterResource(MonsterResource resourceData)
    {
        // 1. 애니메이션 로드 등 부모의 기본 기능을 먼저 실행
        base.ApplyMonsterResource(resourceData);

        if (visualChild != null)
        {
            var sr = visualChild.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.sprite == null)
                {
                    Debug.LogWarning("보스 스프라이트가 비어있습니다! 프리팹의 SpriteRenderer에 이미지를 넣어주세요.");
                }
                else
                {
                    Debug.Log($"보스 이미지 확인됨: {sr.sprite.name} (데이터 테이블 대신 프리팹 설정을 사용합니다)");
                }
            }
        }
    }

    protected override void ApplyAnimatorResource(MonsterResource resourceData)
    {
        Debug.Log($"<color=cyan>[Step 1]</color> 보스 데이터 수신: {resourceData.resource_id} / Cast: {resourceData.cast_animation}");

        // 1. 부모(MonsterBase)가 move_animation을 처리하도록 먼저 실행
        base.ApplyAnimatorResource(resourceData);

        // 2. 보스 전용: 캐스팅(Cast) 애니메이션 로드
        if (!string.IsNullOrEmpty(resourceData.cast_animation))
        {
            Debug.Log($"<color=yellow>[Step 2]</color> 애니메이션 로드 시도 주소: {resourceData.cast_animation}");
            // "CastBase"는 Animator Override Controller에 설정된 원본 클립 이름과 같아야 합니다.
            LoadAndApplyAnimationClip(resourceData.cast_animation, "Cast_Base",
                handle => _castClipHandle = handle);
        }

        // 3. 보스 전용: 공격(Attack) 애니메이션 로드
        if (!string.IsNullOrEmpty(resourceData.attack_animation))
        {
            // "AttackBase" 역시 애니메이터에 설정된 이름 확인 필요
            LoadAndApplyAnimationClip(resourceData.attack_animation, "Attack_Base",
                handle => _attackClipHandle = handle);
        }

        // 4. 사망(Death) 애니메이션도 필요하다면 추가
        if (!string.IsNullOrEmpty(resourceData.death_animation))
        {
            LoadAndApplyAnimationClip(resourceData.death_animation, "Death_Base",
                handle => _deathClipHandle = handle);
        }
    }

    protected override void Start()
    {
        base.Start();

        _laserPattern = new LaserPattern(this);

        if (move_speed == 0f)
            move_speed = base.moveSpeed;

        if (barricadeSpawner == null)
            barricadeSpawner = GetComponent<BarricadeSpawner>();

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.alignment = LineAlignment.View;
        }

        if (zoneSpawner == null)
            zoneSpawner = GetComponent<ZoneSpawner>();

        shooter = GetComponent<BossMonsterShooter>();
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        ResetRuntimeState();

        isDataInitialized = false;
        isWaitingForMonsterId = false;

        Debug.Log($"[BossMonster] OnSpawn / MonsterId:{MonsterId}");

        if (MonsterId <= 0)
        {
            Debug.LogWarning($"[BossMonster] OnSpawn 시점 MonsterId 미설정. 대기 후 초기화 예정 / MonsterId:{MonsterId}");
            isWaitingForMonsterId = true;
            return;
        }

        InitializeBossData();
    }

    private void InitializeBossData()
    {
        if (isDataInitialized)
            return;

        if (MonsterId <= 0)
        {
            Debug.LogError($"[BossMonster] 유효하지 않은 MonsterId: {MonsterId}");
            return;
        }

        isDataInitialized = true;
        isWaitingForMonsterId = false;

        Debug.Log($"[BossMonster] InitializeBossData / MonsterId:{MonsterId}");

        MonsterSO monsterSO = DataManager.Instance.GetSOData<MonsterSO>();
        if (monsterSO == null)
        {
            Debug.LogError("[BossMonster] MonsterSO를 찾을 수 없습니다.");
            return;
        }

        Monster monsterData = monsterSO.GetById(MonsterId);
        if (monsterData == null)
        {
            Debug.LogError($"[BossMonster] 몬스터 데이터 없음. MonsterId:{MonsterId}");
            return;
        }

        pattern_group_id = monsterData.pattern_group_id;

        if (pattern_group_id <= 0)
        {
            Debug.LogWarning($"[BossMonster] pattern_group_id 없음. 기본 보스 동작 사용 / MonsterId:{MonsterId}");
            return;
        }

        PatternGroupCompositionSO compositionSO = DataManager.Instance.GetSOData<PatternGroupCompositionSO>();
        if (compositionSO == null)
        {
            Debug.LogError("[BossMonster] PatternGroupCompositionSO를 찾을 수 없습니다.");
            return;
        }

        List<PatternGroupComposition> compositionList = compositionSO.GetAllByGroupId(pattern_group_id);
        if (compositionList == null || compositionList.Count == 0)
        {
            Debug.LogError($"[BossMonster] composition 데이터 없음. pattern_group_id:{pattern_group_id}");
            return;
        }

        PatternSO patternSO = DataManager.Instance.GetSOData<PatternSO>();
        if (patternSO == null)
        {
            Debug.LogError("[BossMonster] PatternSO를 찾을 수 없습니다.");
            return;
        }

        foreach (var composition in compositionList)
        {
            Pattern patternData = patternSO.GetById(composition.pattern_id);
            if (patternData == null)
                continue;

            Debug.Log($"[Boss Pattern 확인] pattern_id:{patternData.pattern_id}, type:{patternData.pattern_type}, logic:{patternData.pattern_logic_type}");

            switch (patternData.pattern_type)
            {
                case PATTERN_TYPE.Move:
                    ApplyBossMovePattern(patternData);
                    break;

                case PATTERN_TYPE.Projectile:
                    ApplyBossProjectilePattern(patternData, composition.pattern_cooldown);
                    break;

                case PATTERN_TYPE.Barricade:
                    ApplyBossBarricadePattern(patternData);
                    break;

                case PATTERN_TYPE.Trigger:
                    Debug.Log($"[BossMonster] Trigger 패턴 감지: {patternData.pattern_logic_type} (아직 미연동)");
                    break;

                case PATTERN_TYPE.Area:
                    ApplyBossAreaPattern(patternData, composition.pattern_cooldown);
                    break;

                case PATTERN_TYPE.Summon:
                    Debug.Log($"[BossMonster] Summon 패턴 감지: {patternData.pattern_logic_type} (아직 미연동)");
                    break;

                case PATTERN_TYPE.Layser:
                    float finalDmg = this.power * (patternData.damage_multiply > 0 ? patternData.damage_multiply : 1.0f);
                    float duration = patternData.duration; // 레이저 지속시간
                    float castTime = patternData.cast_time; // 예고 시간
                    float size = patternData.projectile_size; // 레이저 굵기
                    int count = patternData.projectile_count; // 레이저 개수

                    _laserPattern.OnLaserPattern(finalDmg, duration, castTime, size, count);
                    Debug.Log($"[BossMonster] Laser 패턴 감지: {patternData.pattern_logic_type} (아직 미연동)");
                    break;

                case PATTERN_TYPE.SelfDestruct:
                    Debug.Log($"[BossMonster] SelfDestruct 패턴 감지: {patternData.pattern_logic_type} (보스는 현재 미사용)");
                    break;
            }
        }

        if (shooter != null && !useOrbit && !IsShooterConfigured())
        {
            shooter.DisableShooter();
        }

        jumpCooldownTimer = pattern_cooldown;
        blinkCooldownTimer = blink_cooldown;
        barricadeTimer = barricadeInterval;

        if (!useOrbit)
            orbitCooldownTimer = 0f;

        Debug.Log(
            $"[Boss 패턴 적용 완료] " +
            $"MonsterId:{MonsterId}, PatternGroupId:{pattern_group_id}, " +
            $"Jump:{useJump}, Flee:{useFlee}, Blink:{useBlink}, Barricade:{useBarricade}, Orbit:{useOrbit}, Zigzag:{useZigzag}"
        );
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

        StopAllCoroutines();

        foreach (var orbit in activeOrbits)
        {
            if (orbit != null)
                Destroy(orbit);
        }
        activeOrbits.Clear();

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        if (shooter != null)
            shooter.DisableShooter();

        ResetRuntimeState();

        isDataInitialized = false;
        isWaitingForMonsterId = false;
    }

    protected override void FixedUpdate()
    {
        if (isPaused)
        {
            if (rb2D != null)
                rb2D.linearVelocity = Vector2.zero;
            return;
        }
        if (!isDataInitialized)
        {
            if (MonsterId > 0)
            {
                InitializeBossData();
            }
            else
            {
                return;
            }
        }

        if (target == null || this.hp <= 0)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // --- 패턴 로직 실행 ---
        // 트리거 패턴 중 보스 정지 하기
        if (isAttacking)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }
        // 공전 투사체 패턴
        if (useOrbit && !isOrbiting && orbitCooldownTimer <= 0f)
        {
            Debug.Log("[BossMonster] 공전 패턴 시작");
            Debug.Log($"[Orbit Final] count={orbitCount}, radius={orbitRadius}, scale={orbitProjectileScale}, bossScale={transform.localScale}");
            StartCoroutine(OrbitPatternRoutine());
        }
        else if (orbitCooldownTimer > 0f)
        {
            orbitCooldownTimer -= Time.fixedDeltaTime;
        }

        if (activeOrbits.Count > 0)
        {
            UpdateOrbitPositions();
        }

        if (useBlink)
            HandleBlinkLogic();

        if (useJump && !isBlinking && !isWaitingBlink)
            HandleJumpLogic();

        if (!isJumping && !isWaitingJump && !isBlinking && !isWaitingBlink)
            MoveProcess();

        if (useBarricade)
            HandleBarricadeLogic();

        HandleAreaPatternLogic();
    }

    #region 데이터 적용 함수
    private void ApplyBossMovePattern(Pattern patternData)
    {
        string logic = NormalizeBossMoveLogic(patternData.pattern_logic_type);

        detect_range = patternData.detect_range > 0f ? patternData.detect_range : detect_range;
        cast_time = patternData.cast_time > 0f ? patternData.cast_time : cast_time;
        pattern_cooldown = patternData.cooldown > 0f ? patternData.cooldown : pattern_cooldown;
        pattern_multiply = ResolveMoveMultiply(patternData);
        pattern_damage = power * (patternData.damage_multiply > 0f ? patternData.damage_multiply : 1f);
        jump_height = patternData.jump_height > 0f ? patternData.jump_height : jump_height;

        switch (logic)
        {
            case "Jump":
                useJump = true;
                break;

            case "Flee":
                useFlee = true;
                fleeDistance = detect_range > 0f ? detect_range : fleeDistance;
                break;

            case "Blink":
                useBlink = true;
                blink_detect_range = detect_range > 0f ? detect_range : blink_detect_range;
                blink_cast_time = cast_time > 0f ? cast_time : blink_cast_time;
                blink_cooldown = pattern_cooldown > 0f ? pattern_cooldown : blink_cooldown;
                blink_speed_multiplier = pattern_multiply > 0f ? pattern_multiply : blink_speed_multiplier;
                break;

            case "Zigzag":
                useZigzag = true;
                zigzag_width = patternData.zigzag_width > 0f ? patternData.zigzag_width : zigzag_width;
                break;

            default:
                Debug.LogWarning($"[BossMonster] 지원하지 않는 Move 로직: {patternData.pattern_logic_type}");
                break;
        }
    }

    private void ApplyBossAreaPattern(Pattern patternData, float compositionCooldown = -1f)
    {
        if (zoneSpawner == null)
        {
            zoneSpawner = GetComponent<ZoneSpawner>();
        }

        if (zoneSpawner == null)
        {
            Debug.LogWarning($"[BossMonster] ZoneSpawner 없음. area 패턴 적용 불가 / pattern_id:{patternData.pattern_id}");
            return;
        }

        string logic = patternData.pattern_logic_type?.Trim().ToLower();

        BossAreaPatternRuntime data = new BossAreaPatternRuntime();
        data.isSafeZone = logic == "area_safe";
        data.cooldown = compositionCooldown > 0f
            ? compositionCooldown
            : (patternData.cooldown > 0f ? patternData.cooldown : 3f);

        data.currentCooldown = data.cooldown;
        data.previewTime = patternData.cast_time > 0f ? patternData.cast_time : 1f;
        data.activeTime = patternData.duration > 0f ? patternData.duration : 3f;
        data.radius = patternData.area_radius > 0f ? patternData.area_radius : 3f;

        // 여기 설계 따라 조정 가능
        // 지금은 "보스 공격력 * damage_multiply" 로 처리
        data.damagePerTick = this.power * (patternData.damage_multiply > 0f ? patternData.damage_multiply : 1f);

        data.damageInterval = patternData.area_damage_interval > 0f ? patternData.area_damage_interval : 0.5f;
        data.count = 1;
        data.range = 0f;

        switch (patternData.area_target_type)
        {
            case AREA_TARGET_TYPE.Player:
                data.targetType = ZoneTestCaller.SpawnTarget.Player;
                break;

            case AREA_TARGET_TYPE.Boss:
                data.targetType = ZoneTestCaller.SpawnTarget.Monster;
                break;

            default:
                data.targetType = ZoneTestCaller.SpawnTarget.Player;
                break;
        }

        areaPatterns.Add(data);

        Debug.Log(
            $"[Boss Area 적용 완료] " +
            $"PatternId:{patternData.pattern_id}, Logic:{patternData.pattern_logic_type}, " +
            $"Cooldown:{data.cooldown}, Preview:{data.previewTime}, Active:{data.activeTime}, " +
            $"Radius:{data.radius}, Damage:{data.damagePerTick}, Tick:{data.damageInterval}, " +
            $"Safe:{data.isSafeZone}, Target:{data.targetType}"
        );
    }

    private void HandleAreaPatternLogic()
    {
        if (zoneSpawner == null || target == null || areaPatterns.Count == 0) return;

        // 핵심: 어떤 장판 패턴이라도 "실행 중"이라면 다른 패턴의 쿨타임을 계산하지 않음
        if (isAreaPatternRunning) return;

        for (int i = 0; i < areaPatterns.Count; i++)
        {
            BossAreaPatternRuntime pattern = areaPatterns[i];
            pattern.currentCooldown -= Time.fixedDeltaTime;

            if (pattern.currentCooldown <= 0f)
            {
                // 코루틴을 통해 패턴 실행 및 대기 처리
                StartCoroutine(AreaPatternSequenceCoroutine(pattern));
                break;
            }
        }
    }

    private IEnumerator AreaPatternSequenceCoroutine(BossAreaPatternRuntime pattern)
    {
        isAreaPatternRunning = true; // 다른 장판 패턴이 시작되지 못하게 잠금

        // 패턴 실행
        ExecuteAreaPattern(pattern);

        // 실행 후 즉시 쿨타임 초기화
        pattern.currentCooldown = pattern.cooldown;

        // [중요] 다음 패턴이 나올 때까지 최소 대기 시간 부여
        // 예를 들어 장판이 깔리고 1~2초 정도는 여유를 주고 싶다면:
        yield return new WaitForSeconds(pattern.previewTime + 1.0f);

        isAreaPatternRunning = false; // 잠금 해제
    }

    private void ExecuteAreaPattern(BossAreaPatternRuntime pattern)
    {
        // 스폰 위치 결정 (플레이어 타겟이면 target, 보스 타겟이면 보스 자신)
        Transform spawnTarget = (pattern.targetType == ZoneTestCaller.SpawnTarget.Player) ? target : transform;

        if (pattern.isSafeZone)
        {
            // 안전지대 패턴 실행 (SafeZonePatternController가 있는 경우)
            SafeZonePatternController.Instance?.StartPattern(
                pattern.previewTime, pattern.activeTime, pattern.damagePerTick, pattern.damageInterval);

            // 실제 안전지대 오브젝트 생성
            zoneSpawner.SpawnSafeZones(
                null, spawnTarget, pattern.count, pattern.range, pattern.radius, pattern.previewTime, pattern.activeTime);
        }
        else
        {
            // 위험지대(데미지 존) 오브젝트 생성
            zoneSpawner.SpawnDangerZones(
                null, spawnTarget, pattern.count, pattern.range, pattern.radius, pattern.previewTime, pattern.activeTime,
                pattern.damagePerTick, pattern.damageInterval);
        }
    }

    private void ApplyBossProjectilePattern(Pattern patternData, float compositionCooldown = -1f)
    {
        if (NormalizeProjectileLogic(patternData.pattern_logic_type) == "Orbit")
        {
            useOrbit = true;

            orbitCount = patternData.projectile_count > 0
                ? patternData.projectile_count
                : defaultOrbitCount;

            orbitRadius = patternData.projectile_radius > 0f
                ? patternData.projectile_radius
                : defaultOrbitRadius;

            orbitSpawnInterval = patternData.fire_interval > 0f
                ? patternData.fire_interval
                : defaultOrbitSpawnInterval;

            orbitDuration = patternData.life_time > 0f
                ? patternData.life_time
                : defaultOrbitDuration;

            orbitDamageMultiplier = patternData.damage_multiply > 0f
                ? patternData.damage_multiply
                : defaultOrbitDamageMultiplier;

            //  projectile_speed를 공전 회전 속도로 사용
            orbitRotateSpeed = patternData.projectile_speed > 0f
                ? patternData.projectile_speed * orbitRotateSpeedMultiplier
                : defaultOrbitRotateSpeed;

            // projectile_size를 공전 오브젝트 크기로 사용
            orbitProjectileScale = patternData.projectile_size > 0f
                ? patternData.projectile_size
                : defaultOrbitProjectileScale;

            orbitPatternCooldown = compositionCooldown > 0f
                ? compositionCooldown
                : (patternData.cooldown > 0f ? patternData.cooldown : 3f);

            orbitCooldownTimer = orbitPatternCooldown;

            orbitRadius = Mathf.Max(orbitRadius, GetMinimumOrbitRadius());

            Debug.Log(
                $"[Boss Orbit 적용 완료] " +
                $"PatternId:{patternData.pattern_id}, " +
                $"DataSpeed:{patternData.projectile_speed}, DataSize:{patternData.projectile_size}, " +
                $"Count:{orbitCount}, Radius:{orbitRadius}, RotateSpeed:{orbitRotateSpeed}, " +
                $"SpawnInterval:{orbitSpawnInterval}, Duration:{orbitDuration}, " +
                $"DamageMul:{orbitDamageMultiplier}, Scale:{orbitProjectileScale}, " +
                $"Cooldown:{orbitPatternCooldown}"
            );
            return;
        }

        if (shooter == null)
        {
            shooter = GetComponent<BossMonsterShooter>();
        }

        if (shooter != null)
        {
            shooter.ApplyProjectilePattern(patternData, compositionCooldown);
        }
        else
        {
            Debug.LogWarning($"[BossMonster] BossMonsterShooter 없음. projectile 패턴 적용 불가 / pattern_id:{patternData.pattern_id}");
        }
    }

    private void ApplyBossBarricadePattern(Pattern patternData)
    {
        useBarricade = true;

        barricadeInterval = patternData.cooldown > 0f ? patternData.cooldown : barricadeInterval;
        barDuration = patternData.duration > 0f ? patternData.duration : barDuration;

        barShape = patternData.barrier_shape == BarrierShapeType.Circle
            ? BarricadeSpawner.BarricadeShape.Circle
            : BarricadeSpawner.BarricadeShape.Rectangle;

        barSize = new Vector2(
            patternData.barrier_size_x > 0f ? patternData.barrier_size_x : barSize.x,
            patternData.barrier_size_y > 0f ? patternData.barrier_size_y : barSize.y
        );

        switch (patternData.barrier_target_type)
        {
            case BARRIER_TARGET_TYPE.Player:
                spawnLoc = BarricadeSpawner.SpawnLocation.Player;
                break;
            case BARRIER_TARGET_TYPE.Boss:
                spawnLoc = BarricadeSpawner.SpawnLocation.Boss;
                break;
            case BARRIER_TARGET_TYPE.Both:
                spawnLoc = BarricadeSpawner.SpawnLocation.Both;
                break;
            case BARRIER_TARGET_TYPE.Nobody:
                spawnLoc = BarricadeSpawner.SpawnLocation.None;
                break;
        }

        barInteraction = patternData.barrier_collision_block
            ? BarricadeSpawner.InteractionType.BlockedWithDamage
            : BarricadeSpawner.InteractionType.PassableWithDamage;


        Debug.Log($"[Boss Barrier 적용 완료] " +
            $"PatternId:{patternData.pattern_id}, " +
            $"Cooldown:{barricadeInterval}, Duration:{barDuration}, " +
            $"Shape:{barShape}, Size:{barSize}, SpawnLoc:{spawnLoc}, Interaction:{barInteraction}"
        );



    }
    #endregion

    #region 점멸(Blink) 로직
    private void HandleBlinkLogic()
    {
        if (isBlinking || isWaitingBlink) return;

        if (blinkCooldownTimer > 0f)
        {
            blinkCooldownTimer -= Time.fixedDeltaTime;
            return;
        }

        float dist = Vector2.Distance(target.position, rb2D.position);

        if (dist <= blink_detect_range && !isWaitingBlink)
            StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isWaitingBlink = true;
        rb2D.linearVelocity = Vector2.zero;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = 2;
            lineRenderer.startWidth = blinkLineWidth;
            lineRenderer.endWidth = blinkLineWidth;
            lineRenderer.sortingOrder = 100;
        }

        float elapsedCast = 0f;
        while (elapsedCast < blink_cast_time)
        {
            while (isPaused)
                yield return null;

            elapsedCast += Time.deltaTime;

            if (lineRenderer != null && target != null)
            {
                Vector3 s = transform.position;
                Vector3 e = target.position;
                s.z = transform.position.z;
                e.z = transform.position.z;

                lineRenderer.SetPosition(0, s);
                lineRenderer.SetPosition(1, e);
            }

            yield return null;
        }

        Vector2 moveStart = rb2D.position;
        Vector2 moveTarget = target.position;

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        isWaitingBlink = false;
        isBlinking = true;

        float distance = Vector2.Distance(moveStart, moveTarget);
        float duration = distance / (move_speed * blink_speed_multiplier);
        float elapsed = 0f;

        while (elapsed < duration && this.hp > 0)
        {
            while (isPaused)
                yield return new WaitForFixedUpdate();

            elapsed += Time.fixedDeltaTime;
            float p = elapsed / duration;
            rb2D.MovePosition(Vector2.Lerp(moveStart, moveTarget, p));
            yield return new WaitForFixedUpdate();
        }

        isBlinking = false;
        blinkCooldownTimer = blink_cooldown;
    }
    #endregion

    #region 공전 투사체(Orbit) 로직
    private IEnumerator OrbitPatternRoutine()
    {
        if (orbitPrefab == null)
        {
            Debug.LogWarning("[BossMonster] orbitPrefab이 없습니다. 공전 패턴을 실행할 수 없습니다.");
            isOrbiting = false;
            yield break;
        }

        isOrbiting = true;

        foreach (var oldOrbit in activeOrbits)
        {
            if (oldOrbit != null)
            {
                Destroy(oldOrbit);
            }
        }
        activeOrbits.Clear();

        for (int i = 0; i < orbitCount; i++)
        {
            if (hp <= 0)
            {
                isOrbiting = false;
                yield break;
            }

            GameObject orbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity);
            orbit.transform.localScale = Vector3.one * orbitProjectileScale;

            var proj = orbit.GetComponent<OrbitProjectile>();
            if (proj != null)
            {
                proj.SetDamage(power * orbitDamageMultiplier);
            }

            activeOrbits.Add(orbit);
            StartCoroutine(DestroyOrbitAfterTime(orbit, orbitDuration));

            yield return new WaitForSeconds(orbitSpawnInterval);
        }

        orbitCooldownTimer = orbitPatternCooldown;
        isOrbiting = false;
    }

    private void UpdateOrbitPositions()
    {
        activeOrbits.RemoveAll(item => item == null);

        int currentCount = activeOrbits.Count;
        if (currentCount == 0) return;

        for (int i = 0; i < currentCount; i++)
        {
            float angle = (i * (360f / Mathf.Max(1, currentCount))) + (Time.time * orbitRotateSpeed);
            float rad = angle * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * orbitRadius;
            activeOrbits[i].transform.position = (Vector2)transform.position + offset;
        }
    }

    private IEnumerator DestroyOrbitAfterTime(GameObject orbit, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (orbit != null)
        {
            activeOrbits.Remove(orbit);
            if (explosionEffect != null)
                Instantiate(explosionEffect, orbit.transform.position, Quaternion.identity);

            Destroy(orbit);
        }
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPaused) return;

        if (isBlinking && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(pattern_damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isPaused) return;

        if (isBlinking && other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(pattern_damage);
        }
    }

    private void HandleBarricadeLogic()
    {
        // 이미 소환 대기 중이거나 일시정지 상태면 스킵
        if (isWaitingBarricade || isPaused) return;

        if (barricadeTimer > 0f)
        {
            barricadeTimer -= Time.fixedDeltaTime;
            return;
        }

        // 타이머가 다 되면 코루틴 실행
        StartCoroutine(BarricadePatternRoutine());
    }

    private Vector2 GetSpawnPosition(BarricadeSpawner.SpawnLocation loc)
    {
        if (target == null) return rb2D.position;

        switch (loc)
        {
            case BarricadeSpawner.SpawnLocation.Player:
                return target.position;

            case BarricadeSpawner.SpawnLocation.Boss:
                return rb2D.position;

            case BarricadeSpawner.SpawnLocation.Both:
                return (rb2D.position + (Vector2)target.position) / 2f;

            case BarricadeSpawner.SpawnLocation.None:
                Camera cam = Camera.main;
                if (cam == null) return rb2D.position;

                float h = cam.orthographicSize;
                float w = h * cam.aspect;
                return (Vector2)cam.transform.position + new Vector2(
                    Random.Range(-w * 0.9f, w * 0.9f),
                    Random.Range(-h * 0.9f, h * 0.9f)
                );

            default:
                return rb2D.position;
        }
    }

    private Vector2 GetSeparationDir(Vector2 myPos)
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

    private Vector2 ApplyAvoidance(Vector2 myPos, Vector2 moveDir)
    {
        if (moveDir.sqrMagnitude <= 0.0001f)
            return Vector2.zero;

        Vector2 separationDir = GetSeparationDir(myPos);
        Vector2 finalDir = moveDir + separationDir * avoidanceForce;

        if (finalDir.sqrMagnitude <= 0.0001f)
            return moveDir.normalized;

        return finalDir.normalized;
    }

    private void MoveProcess()
    {
        Vector2 myPos = rb2D.position;
        float dist = Vector2.Distance(target.position, myPos);

        if (useZigzag)
        {
            zigzagTimer += Time.fixedDeltaTime;

            Vector2 toTarget = (Vector2)target.position - myPos;
            float distToTarget = toTarget.magnitude;

            if (distToTarget < 0.1f)
            {
                rb2D.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 forward = toTarget.normalized;
            Vector2 side = new Vector2(-forward.y, forward.x);

            float sideOffset = Mathf.Sin(zigzagTimer * zigzagFrequency) * zigzag_width;
            float damping = Mathf.Clamp01((distToTarget - 0.2f) / 0.8f);

            float zigzagSpeed = move_speed * pattern_multiply;
            Vector2 movement = (forward * zigzagSpeed) + (side * sideOffset * zigzagFrequency * damping);

            Vector2 finalDir = ApplyAvoidance(myPos, movement.normalized);
            rb2D.linearVelocity = finalDir * zigzagSpeed;
            return;
        }

        Vector2 dir;

        if (useFlee && !isFleeing && dist < fleeDistance && fleeCooldownTimer <= 0f)
        {
            currentFleeTarget = GetSmartFleePosition();
            isFleeing = true;
        }

        if (isFleeing)
        {
            dir = (currentFleeTarget - myPos).normalized;

            if (Vector2.Distance(myPos, currentFleeTarget) < 0.5f)
            {
                isFleeing = false;
                fleeCooldownTimer = pattern_cooldown;
            }
        }
        else
        {
            dir = ((Vector2)target.position - myPos).normalized;

            if (fleeCooldownTimer > 0f)
                fleeCooldownTimer -= Time.fixedDeltaTime;
        }

        float finalSpeed = isFleeing ? move_speed * pattern_multiply : move_speed;
        Vector2 finalDir2 = ApplyAvoidance(myPos, dir);
        rb2D.linearVelocity = finalDir2 * finalSpeed;
    }

    public override void TakeDamage(float damage)
    {
        hp -= damage;
        Debug.Log(damage + "실제로 받는 데미지");

        if (hp <= 0)
            Die();

        _onBossHit.OnStartEvent(hp);
    }

    public void HealBoss(float amount)
    {
        if (amount <= 0f) return;

        hp = Mathf.Min(maxHp, hp + amount);
        _onBossHit?.OnStartEvent(hp);
    }

    public void RefreshBossHpUI()
    {
        _onBossHit?.OnStartEvent(hp);
    }
    private void HandleJumpLogic()
    {
        if (isJumping || isWaitingJump) return;

        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
            return;
        }

        if (Vector2.Distance(target.position, rb2D.position) <= detect_range)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator BarricadePatternRoutine()
    {
        isWaitingBarricade = true; // 소환 프로세스 시작

        // cast_time만큼 대기 (이 시간 동안 보스는 MoveProcess에 의해 계속 이동함)
        float elapsed = 0f;
        while (elapsed < cast_time)
        {
            if (!isPaused) // 일시정지 체크
            {
                elapsed += Time.deltaTime;
            }
            yield return null;
        }

        // 예고 시간이 끝난 '지금' 위치를 확정 
        Vector2 spawnPosition = GetSpawnPosition(spawnLoc);

        // 실제 소환
        if (barricadeSpawner != null)
        {
            barricadeSpawner.SpawnBarricade(
                spawnPosition,
                barShape,
                barSize,
                barDuration,
                barInteraction,
                this.power
            );
        }

        // 상태 초기화 및 쿨타임 설정
        isWaitingBarricade = false;
        barricadeTimer = barricadeInterval;
    }

    private IEnumerator JumpRoutine()
    {
        isWaitingJump = true;
        rb2D.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(cast_time);

        isWaitingJump = false;
        isJumping = true;

        Vector2 start = rb2D.position;
        Vector2 dest = target.position;

        float dur = Vector2.Distance(start, dest) / (move_speed * pattern_multiply);
        float elp = 0f;

        while (elp < dur && this.hp > 0)
        {
            while (isPaused)
                yield return new WaitForFixedUpdate();

            elp += Time.fixedDeltaTime;
            float p = elp / dur;

            rb2D.MovePosition(Vector2.Lerp(start, dest, p));
            if (visualChild != null)
                visualChild.localPosition = new Vector3(0f, Mathf.Sin(p * Mathf.PI) * jump_height, visualChild.localPosition.z);

            yield return new WaitForFixedUpdate();
        }

        if (visualChild != null)
            visualChild.localPosition = Vector2.zero;

        isJumping = false;
        jumpCooldownTimer = pattern_cooldown;
    }

    private Vector2 GetSmartFleePosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return rb2D.position;

        float h = 2f * cam.orthographicSize;
        float w = h * cam.aspect;

        Vector2 camP = (Vector2)cam.transform.position;
        Vector2[] areas = new Vector2[6];
        int[] counts = new int[6];

        for (int i = 0; i < 6; i++)
        {
            float x = (i < 3) ? camP.x - (w / 4f) : camP.x + (w / 4f);
            float y = camP.y + (h / 3f) * (1 - (i % 3));
            areas[i] = new Vector2(x, y);
        }

        foreach (var m in GameObject.FindGameObjectsWithTag("Monster"))
        {
            if (m == gameObject) continue;

            float minD = float.MaxValue;
            int cA = -1;

            for (int j = 0; j < 6; j++)
            {
                float d = Vector2.Distance(m.transform.position, areas[j]);
                if (d < minD)
                {
                    minD = d;
                    cA = j;
                }
            }

            if (cA != -1)
                counts[cA]++;
        }

        int best = 0;
        int maxC = -1;
        float maxD = -1f;

        for (int i = 0; i < 6; i++)
        {
            float dP = Vector2.Distance(areas[i], target.position);

            if (counts[i] > maxC || (counts[i] == maxC && dP > maxD))
            {
                maxC = counts[i];
                maxD = dP;
                best = i;
            }
        }

        return areas[best];
    }

    private void ResetRuntimeState()
    {
        useJump = false;
        useFlee = false;
        useBlink = false;
        useBarricade = false;
        useOrbit = false;
        useZigzag = false;

        isJumping = false;
        isWaitingJump = false;
        isFleeing = false;
        isBlinking = false;
        isWaitingBlink = false;
        isOrbiting = false;

        jumpCooldownTimer = 0f;
        fleeCooldownTimer = 0f;
        blinkCooldownTimer = 0f;
        barricadeTimer = 0f;
        orbitCooldownTimer = 0f;
        zigzagTimer = 0f;

        pattern_cooldown = 0f;
        cast_time = 0f;
        detect_range = 0f;
        pattern_multiply = 1f;
        pattern_damage = 0f;

        if (move_speed <= 0f)
            move_speed = moveSpeed;

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }

        foreach (var orbit in activeOrbits)
        {
            if (orbit != null)
                Destroy(orbit);
        }
        activeOrbits.Clear();

        if (lineRenderer != null)
            lineRenderer.enabled = false;

        areaPatterns.Clear();

        // Orbit 런타임 값 초기화
        orbitCount = defaultOrbitCount;
        orbitRadius = defaultOrbitRadius;
        orbitRotateSpeed = defaultOrbitRotateSpeed;
        orbitDuration = defaultOrbitDuration;
        orbitSpawnInterval = defaultOrbitSpawnInterval;
        orbitDamageMultiplier = defaultOrbitDamageMultiplier;
        orbitProjectileScale = defaultOrbitProjectileScale;
        orbitPatternCooldown = 3f;
    }

    private float ResolveMoveMultiply(Pattern patternData)
    {
        if (patternData == null) return 1f;

        if (patternData.rush_speed > 0f)
            return patternData.rush_speed;

        if (patternData.stat_value > 0f)
            return patternData.stat_value;

        return 1f;
    }

    private string NormalizeBossMoveLogic(string rawLogic)
    {
        if (string.IsNullOrWhiteSpace(rawLogic))
            return string.Empty;

        string logic = rawLogic.Trim().ToLower();

        switch (logic)
        {
            case "move_jump":
            case "jump":
                return "Jump";

            case "move_escape":
            case "move_flee":
            case "flee":
                return "Flee";

            case "move_blink":
            case "blink":
                return "Blink";

            case "move_zigzag":
            case "zigzag":
                return "Zigzag";

            default:
                return string.Empty;
        }
    }

    private string NormalizeProjectileLogic(string rawLogic)
    {
        if (string.IsNullOrWhiteSpace(rawLogic))
            return string.Empty;

        string logic = rawLogic.Trim().ToLower();

        switch (logic)
        {
            case "projectile_turn":
                return "Orbit";
            default:
                return logic;
        }
    }

    private bool IsShooterConfigured()
    {
        return shooter != null && shooter.enabled;
    }

    public override void SetPaused(bool paused)
    {
        base.SetPaused(paused);

        if (paused)
        {
            isBlinking = false;
            isWaitingBlink = false;
            isJumping = false;
            isWaitingJump = false;
            isFleeing = false;

            if (lineRenderer != null)
                lineRenderer.enabled = false;
        }
    }
}