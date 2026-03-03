using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public abstract class SpecialBossMonsterBase : MonsterBase
{
    protected static List<SpecialBossMonsterBase> activeMonsters = new List<SpecialBossMonsterBase>();

    protected enum MonsterState { MOVE, PATTERN }
    protected MonsterState currentState = MonsterState.MOVE;


    [Header("--- monster_table 연동 ---")]
    public float move_speed;               // 몬스터의 기본 이동 속도 
    public float monster_power;            // 플레이어 충돌 시 적용 데미지
    public float detect_range;             //플레이어를 감지하거나 패턴을 발동하는 기준 거리


    [Header("--- pattern_table 연동 ---")]
    public float pattern_cooldown;         // 패턴 대기 시간
    public float cast_time;                // 패턴 발동 전 대기 시간
    public float pattern_multiply;         // 패턴 중 가속 배율
    public float explosion_range;          // 자폭/광역 공격의 물리적 타격 반경
    public float pattern_damage;           // 패턴(자폭 등) 성공 시 플레이어에게 주는 데미지
    public float zigzag_width;             // 좌우 이동 폭
    public float patternFrequency = 5.0f;  // 지그재그 주기

    [Header("--- 이동 특수 패턴 설정 ---")]
    public MonsterMove moveType;           // 이 몬스터가 어떤 이동 패턴을 쓸지 결정

    [Header("--- 점프 전용 상세 설정 ---")]
    public float jump_height = 2.0f;       // 시각적 높이
    public Transform visualChild;

    // --- 내부 제어용 변수 (데이터 테이블 비포함) ---
    protected float patternTimer = 0f;     // 지그재그용 타이머
    protected float stateTimer = 0f;       // 쿨타임 및 시전 대기 타이머
    protected bool isWaiting = false;      // 시전 대기 중(cast_time 체크용)
    protected bool isJumping = false;      // 점프 여부
    private bool hasHitTargetInCurrentJump = false; // 중복 데미지 방지용
    private Vector2 smoothedDir;

    private Vector2 jumpStartPos;          // 점프 시작 지점
    private Vector2 jumpTargetPos;         // 점프 작지 시점
    private float jumpProgress = 0f;       // 점프 진행도 (0~1)

    protected bool isFleeingState = false; // 도망 여부
    protected bool isWaitingFlee = false;  // 도망 대기 여부
    private Vector2 currentFleeTargetPos;  // 선택된 도망 목표 지점

    public bool isSuicideUnit = false;     // 자폭 여부
    private bool hasExploded = false;      // 이미 터졌는지 체크 (중복 실행 방지)
    private float currentCastTimer;        // 실시간 자폭 대기 타이머

    

    protected override void Start()
    {
        base.Start();

        // 스폰 시 기본 속도 설정
        if (move_speed == 0) move_speed = base.moveSpeed;

        // 자폭 유닛인 경우 타이머 초기화
        if (isSuicideUnit)
        {
            currentCastTimer = cast_time; // cast_time 사용

            currentState = MonsterState.PATTERN;
        }
    }

    protected override void FixedUpdate()
    {
        if (target == null) { Debug.LogError($"{gameObject.name}: 타겟(플레이어)이 없습니다!"); return; }
        if (hasExploded) return;

        if (this.hp <= 0)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (isSuicideUnit)
        {
            if (currentCastTimer > 0)
            {
                currentCastTimer -= Time.fixedDeltaTime;
                MoveToTarget();
            }
            else
            {
                ExecuteExplosion();
                return;
            }
        }
        else
        {
            // 일반 유닛: 공격 중이 아닐 때만 이동
            if (!isAttacking)
            {
                MoveToTarget();
            }
        }
    }

    protected void OnEnable()
    {
        hasExploded = false;           // 자폭 여부 초기화
        currentCastTimer = cast_time;  // 자폭 타이머 초기화
        stateTimer = 0f;               // 패턴 쿨타임 초기화
        isJumping = false;             // 점프 상태 초기화
        isFleeingState = false;        // 도망 상태 초기화
        isWaiting = false;
        isWaitingFlee = false;
        hasHitTargetInCurrentJump = false;

        // --- 리스트 관리 ---
        if (!activeMonsters.Contains(this))
            activeMonsters.Add(this);
    }

    protected void OnDisable()
    {
        activeMonsters.Remove(this);
    }

    protected override void MoveToTarget()
    {
        patternTimer += Time.fixedDeltaTime;
        stateTimer += Time.fixedDeltaTime;

        // 자폭 유닛 전용 이동 
        if (isSuicideUnit && !hasExploded)
        {
            float suicideSpeed = move_speed * pattern_multiply;
            Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;
            float dist = Vector2.Distance(target.position, rb2D.position);

            if (dist > 0.1f)
                rb2D.linearVelocity = dir * suicideSpeed;
            else
                rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // 일반 유닛 이동 로직
        float originalSpeed = move_speed;

        // 점프 중일 때만 속도 배율 적용 (다른 패턴은 개별 함수에서 처리)
        if (isJumping) move_speed *= pattern_multiply;

        switch (moveType)
        {
            case MonsterMove.StraightChase: StraightChase(); break;
            case MonsterMove.Zigzag: ZigzagMove(); break;
            case MonsterMove.Jump: JumpMove(); break;
            case MonsterMove.Flee: FleeMove(); break;
            default: StraightChase(); break;
        }

        move_speed = originalSpeed;
    }

    protected void ZigzagMove()
    {
        Vector2 myPos = rb2D.position;
        Vector2 targetPos = (Vector2)target.position;
        Vector2 toTarget = targetPos - myPos;
        float dist = toTarget.magnitude;

        // 플레이어를 향한 기준선 
        Vector2 baselineDir = (dist > 0.1f) ? toTarget.normalized : rb2D.linearVelocity.normalized;

        // 수직 방향
        Vector2 sideDir = new Vector2(-baselineDir.y, baselineDir.x);

        // 지그재그 계산
        float sideOffset = Mathf.Sin(patternTimer * patternFrequency) * zigzag_width;

        // 거리 기반 감쇄 (Damping)
        
        float damping = Mathf.Clamp01((dist - 0.2f) / 0.8f);

        // 최종 이동 계산
        Vector2 movement = (baselineDir * move_speed) + (sideDir * sideOffset * patternFrequency * damping);

        // 속도 적용 
        if (dist < 0.1f)
        {
            rb2D.linearVelocity = Vector2.zero;
        }
        else
        {
            rb2D.linearVelocity = movement;
        }
    }


    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSuicideUnit) return;

        if (collision.CompareTag("Player"))
        {
            IDamageable player = collision.GetComponent<IDamageable>();
            if (player == null) return;

            // 지그재그 패턴 데미지
            if (moveType == MonsterMove.Zigzag)
            {
                player.TakeDamage(this.monster_power);
                Debug.Log("지그재그 데미지 적용!");
                return;
            }

            // 점프 도중 충돌 시 데미지
            if (moveType == MonsterMove.Jump && isJumping && !hasHitTargetInCurrentJump)
            {
                player.TakeDamage(this.pattern_damage);
                hasHitTargetInCurrentJump = true; // (중복 방지)
                Debug.Log("점프 충돌 데미지 적용!");
                return;
            }

            player.TakeDamage(this.monster_power);

            // 어떤 상태에서 부딪혔는지 명확히 로그 찍기
            //if (moveType == MonsterMove.Flee && isFleeingState)
            //    Debug.Log("<color=magenta>[도망 중]</color> 충돌 데미지!");
            //else if (moveType == MonsterMove.Flee && !isFleeingState)
            //    Debug.Log("<color=white>[도망유닛-추격중]</color> 접촉 데미지!");
            //else if (moveType == MonsterMove.StraightChase)
            //    Debug.Log("<color=white>[일반추격]</color> 접촉 데미지!");
            //else
            //    Debug.Log($"<color=gray>[기본접촉]</color> 상태: {moveType}");

        }
    }

    // 점프
    protected void JumpMove()
    {
        float distance = Vector2.Distance(target.position, rb2D.position);

        if (!isJumping && !isWaiting)
        {
            // 사거리 보다 멀거나, 아직 쿨타임이 안 찼다면?
            if (distance > detect_range || stateTimer < pattern_cooldown)
            {
                // 플레이어를 향해 걸어감
                StraightChase();
                return;
            }

            // 사거리 안쪽이고 쿨타임도 다 찼다면? -> 점프 준비
            currentState = MonsterState.PATTERN;
            isWaiting = true;
            stateTimer = 0;
            rb2D.linearVelocity = Vector2.zero; // 점프 전 멈춤
            return;
        }

        if (isWaiting)
        {
            if (stateTimer >= cast_time)
            {
                isWaiting = false;
                isJumping = true;
                hasHitTargetInCurrentJump = false; // 점프 시작 시 초기화
                jumpStartPos = rb2D.position;
                jumpTargetPos = target.position;
                jumpProgress = 0f;
                stateTimer = 0;
            }
            return;
        }

        if (isJumping)
        {
            float actualSpeed = move_speed * pattern_multiply;
            float totalDistance = Vector2.Distance(jumpStartPos, jumpTargetPos);
            float duration = (totalDistance > 0) ? totalDistance / actualSpeed : 0.1f;

            jumpProgress += Time.fixedDeltaTime / duration;

            if (this.hp <= 0)
            {
                if (visualChild != null) visualChild.localPosition = Vector2.zero;
                rb2D.linearVelocity = Vector2.zero;
                isJumping = false;
                return;
            }

            if (jumpProgress >= 1f)
            {
                // 착지 시점
                rb2D.linearVelocity = Vector2.zero;
                rb2D.position = jumpTargetPos;

                isJumping = false;
                currentState = MonsterState.MOVE;
                stateTimer = 0;

                if (visualChild != null) visualChild.localPosition = Vector2.zero;
            }
            else
            {
                Vector2 nextTargetPos = Vector2.Lerp(jumpStartPos, jumpTargetPos, jumpProgress);
                Vector2 moveDir = (nextTargetPos - rb2D.position);

                rb2D.linearVelocity = moveDir / Time.fixedDeltaTime;

                if (visualChild != null)
                {  
                    float currentHeight = Mathf.Sin(jumpProgress * Mathf.PI) * jump_height;
                    visualChild.localPosition = new Vector2(0, currentHeight);
                }
            }
        }
    }

    // 도망
    protected void FleeMove()
    {
        if (this.hp <= 0) { CompleteFleePattern(); return; }

        float distanceToPlayer = Vector2.Distance(target.position, rb2D.position);

        // 일반 추격 중
        if (!isFleeingState && !isWaitingFlee)
        {
            if (distanceToPlayer > detect_range || stateTimer < pattern_cooldown)
            {
                StraightChase();
                return;
            }
            else // 도망 발동 조건 만족
            {
                rb2D.linearVelocity = Vector2.zero; // 즉시 정지 (지나침 방지)
                isWaitingFlee = true;
                stateTimer = 0;
                return;
            }
        }

        if (isWaitingFlee)
        {
            rb2D.linearVelocity = Vector2.zero; // 대기 중엔 확실히 멈춤
            if (stateTimer >= cast_time)
            {
                isWaitingFlee = false;
                isFleeingState = true;
                stateTimer = 0;
                currentFleeTargetPos = GetSmartFleePosition(); // 도망 지점 결정
            }
            return;
        }

        // 실제 도망 중
        if (isFleeingState)
        {
            Vector2 dir = (currentFleeTargetPos - rb2D.position).normalized;
            float distToTarget = Vector2.Distance(rb2D.position, currentFleeTargetPos);

            // 도망 속도 적용
            rb2D.linearVelocity = dir * (move_speed * pattern_multiply);

            // 도착 체크 또는 플레이어가 도망 지점에 너무 가까워지면 패턴 종료
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
        rb2D.linearVelocity = Vector2.zero; // 도착 시 속도 제거

        // monster.state = MOVE 복귀
        currentState = MonsterState.MOVE;

        // pattern_group_composition_table.pattern_cooldown 초기화
        stateTimer = 0;
    }

    private Vector2 GetSmartFleePosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return rb2D.position;

        // 카메라 영역 계산 
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

        // "Monster" 태그를 가진 모든 오브젝트 찾기
        GameObject[] allMonsters = GameObject.FindGameObjectsWithTag("Monster");

        // 디버그 
        //Debug.Log($"[Flee] 주변에 인식된 총 몬스터(태그 기준): {allMonsters.Length}");

        foreach (var mObj in allMonsters)
        {
            // 자기 자신은 제외
            if (mObj == this.gameObject) continue;

            float minDist = float.MaxValue;
            int closestArea = -1;

            // 이 오브젝트가 6개 영역 중 어디와 가장 가까운지 계산
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

        // 최적 영역 선택 
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

    // 자폭
    private void ExecuteExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Debug.Log("자폭 발동!");

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb2D.position, explosion_range);
        // bool hitPlayer = false; // 자폭 확인용 

        foreach (var hit in hits)
        {

            if (hit.CompareTag("Player"))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(this.pattern_damage);
                    // Debug.Log($"데미지 적중");
                    // hitPlayer = true;
                }
                break;
            }
        }
        // if (!hitPlayer) Debug.Log("자폭 빗나감");

        // 풀에 들어가기 전 정지
        rb2D.linearVelocity = Vector2.zero;

        // Destroy 대신 풀링 매니저에 반납
        if (TryGetComponent<PoolObject>(out var poolObj))
        {
            poolObj.ReturnToPool();
        }
        else
        {
            Destroy(gameObject); // 혹시 모를 예외 상황 대비
        }
    }

    private void UpdateWarningVisuals(float progressNormalized) { }

    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
        if (isAttacking && rb2D != null) rb2D.linearVelocity = Vector2.zero;
    }

    protected new void StraightChase()
    {
        Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;
        float distance = Vector2.Distance(target.position, rb2D.position);

        if (distance > 0.1f)
        {
            rb2D.linearVelocity = dir * move_speed;
        }
        else
        {
            // 아주 가까우면 속도를 0으로 만들어 떨림 방지
            rb2D.linearVelocity = Vector2.zero;
        }
    }
}