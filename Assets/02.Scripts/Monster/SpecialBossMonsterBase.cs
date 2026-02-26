using UnityEngine;
using System.Collections;
using System.Linq;
using MonsterEnum;

public abstract class SpecialBossMonsterBase : MonsterBase
{
    
    [Header("--- monster_table 연동 ---")]
    public float move_speed;               // 몬스터의 기본 이동 속도 
    public float detect_range;             //플레이어를 감지하거나 패턴을 발동하는 기준 거리


    [Header("--- pattern_table 연동 ---")]
    public float cast_time;                // 패턴 발동 전 대기 시간
    public float pattern_multiply;         // 패턴 중 가속 배율
    public float explosion_range;          // 자폭/광역 공격의 물리적 타격 반경
    public float pattern_damage;           // 패턴(자폭 등) 성공 시 플레이어에게 주는 데미지

    [Header("--- 이동 특수 패턴 설정 ---")]
    public MonsterMove moveType;           // 이 몬스터가 어떤 이동 패턴을 쓸지 결정
    public float patternAmplitude = 5.0f;  // 지그재그 진폭
    public float patternFrequency = 5.0f;  // 지그재그 주기

    [Header("--- 점프 전용 상세 설정 (기획 외 수치) ---")]
    public float jump_height = 2.0f;       // 시각적 높이
    public float jumpCooldown = 3.0f;      // 점프 패턴 간의 재사용 대기 시간

    [Header("--- 시각 효과 설정 ---")]
    public Transform visualChild;          // 점프 시 높이 표현용 자식 오브젝트

    // --- 내부 제어용 변수 (데이터 테이블 비포함) ---
    protected float patternTimer = 0f;     // 지그재그요 타이머
    protected float stateTimer = 0f;       // 쿨타임 및 시전 대기 타이머
    protected bool isWaiting = false;      // 시전 대기 중(cast_time 체크용)
    protected bool isFleeingState = false; // 도망 여부
    protected bool isWaitingFlee = false;  // 도망 대기 여부
    protected bool isJumping = false;      // 점프 여부

    private Vector2 jumpStartPos;          // 점프 시작 지점
    private Vector2 jumpTargetPos;         // 점프 작지 시점
    private float jumpProgress = 0f;       // 점프 진행도 (0~1)

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
            currentCastTimer = cast_time; // 기획서 cast_time 사용
        }
    }

    protected override void FixedUpdate()
    {
        if (target == null) { Debug.LogError($"{gameObject.name}: 타겟(플레이어)이 없습니다!"); return; }
        if (hasExploded) return;

        if (isSuicideUnit)
        {
            //Debug.Log($"{gameObject.name}: 자폭 추적 중 - 타이머: {currentCastTimer}, 속도배율: {pattern_multiply}");

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

    protected override void MoveToTarget()
    {
        patternTimer += Time.fixedDeltaTime;
        stateTimer += Time.fixedDeltaTime;

        // 자폭 유닛 전용 이동 (가장 우선순위 높음)
        if (isSuicideUnit && !hasExploded)
        {
            float suicideSpeed = move_speed * pattern_multiply;
            Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;

            // 이동 실행
            rb2D.MovePosition(rb2D.position + dir * suicideSpeed * Time.fixedDeltaTime);
            return;
        }

        // 일반 유닛 이동 로직
        float originalSpeed = move_speed;
        if (isJumping || isFleeingState) move_speed *= pattern_multiply;

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

    // --- [패턴 로직들] ---


    // 지그재그
    protected void ZigzagMove()
    {
        Vector2 myPos = rb2D.position;
        Vector2 forwardDir = ((Vector2)target.position - myPos).normalized; // 앞쪽 방향
        Vector2 sideDir = new Vector2(-forwardDir.y, forwardDir.x); // 옆쪽(수직) 방향

        // 시간 흐름에 따라 좌우 오프셋 계산
        float sideOffset = Mathf.Sin(patternTimer * patternFrequency) * patternAmplitude;

        Vector2 movement = (forwardDir * move_speed * Time.fixedDeltaTime) +
                           (sideDir * sideOffset * Time.fixedDeltaTime);
        rb2D.MovePosition(myPos + movement);
    }

    // 점프
    protected void JumpMove()
    {
        float distance = Vector2.Distance(target.position, rb2D.position);

        // 점프 조건 체크
        if (!isJumping && !isWaiting && stateTimer >= jumpCooldown && distance <= detect_range)
        {
            isWaiting = true;
            stateTimer = 0;
            rb2D.linearVelocity = Vector2.zero; // 점프 전 멈춤
            return;
        }

        // 점프 시전 대기
        if (isWaiting)
        {
            if (stateTimer >= cast_time)
            {
                isWaiting = false;
                isJumping = true;
                jumpStartPos = rb2D.position;
                jumpTargetPos = target.position; // 점프 시작 시점의 타겟 위치 저장
                jumpProgress = 0f;
                stateTimer = 0;
            }
            return;
        }

        // 점프 실행 중
        if (isJumping)
        {
            float actualSpeed = move_speed;
            float totalDistance = Vector2.Distance(jumpStartPos, jumpTargetPos);
            float duration = (totalDistance > 0) ? totalDistance / actualSpeed : 0.1f;
            jumpProgress += Time.fixedDeltaTime / duration;

            if (jumpProgress >= 1f) // 착지
            {
                rb2D.MovePosition(jumpTargetPos);
                isJumping = false;
                stateTimer = 0;
                if (visualChild != null) visualChild.localPosition = Vector2.zero;
            }
            else // 공중 이동 중
            {
                Vector2 nextPos = Vector2.Lerp(jumpStartPos, jumpTargetPos, jumpProgress);
                rb2D.MovePosition(nextPos);

                // Sin 함수를 이용해 visualChild만 위로 띄워 포물선 표현
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
        float distanceToPlayer = Vector2.Distance(target.position, rb2D.position);

        // 도망 발동 체크
        if (!isFleeingState && !isWaitingFlee && stateTimer >= jumpCooldown && distanceToPlayer <= detect_range)
        {
            isWaitingFlee = true;
            stateTimer = 0;
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // 도망 전 대기
        if (isWaitingFlee)
        {
            if (stateTimer >= cast_time)
            {
                isWaitingFlee = false;
                isFleeingState = true;
                stateTimer = 0;
            }
            return;
        }

        // 도망 실행
        if (isFleeingState)
        {
            Vector2 targetAreaPos = GetSmartFleePosition(); // 안전한 지점 계산
            Vector2 myPos = rb2D.position;
            Vector2 dir = (targetAreaPos - myPos).normalized;
            rb2D.MovePosition(myPos + dir * move_speed * Time.fixedDeltaTime);

            // 목적지 근처에 도착하면 상태 해제
            if (Vector2.Distance(myPos, targetAreaPos) < 0.5f)
            {
                isFleeingState = false;
                stateTimer = 0;
            }
        }
    }

    // 몬스터 밀집 지역 계산 
    private Vector2 GetSmartFleePosition()
    {
        Camera cam = Camera.main;
        if (cam == null) return rb2D.position;

        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;
        Vector2 camPos = cam.transform.position;

        Vector2[] areaCenters = new Vector2[6];
        int[] monsterCounts = new int[6];

        // 6개 영역의 중심점 계산
        for (int i = 0; i < 6; i++)
        {
            float x = (i < 3) ? camPos.x - width / 4 : camPos.x + width / 4;
            float y = camPos.y + (height / 3) * (1 - (i % 3));
            areaCenters[i] = new Vector2(x, y);
        }

        // "Monster" 태그를 가진 객체들을 찾아 각 영역별 인원수 파악
        var allMonsters = GameObject.FindGameObjectsWithTag("Monster");
        foreach (var m in allMonsters)
        {
            float minDist = float.MaxValue;
            int closestArea = 0;
            for (int i = 0; i < 6; i++)
            {
                float d = Vector2.Distance(m.transform.position, areaCenters[i]);
                if (d < minDist) { minDist = d; closestArea = i; }
            }
            monsterCounts[closestArea]++;
        }

        // 가장 안전한(몬스터가 많고 플레이어와 먼) 영역 선택
        int bestAreaIndex = 0;
        int maxCount = -1;
        float maxPlayerDist = -1f;

        for (int i = 0; i < 6; i++)
        {
            float distToPlayer = Vector2.Distance(areaCenters[i], target.position);
            if (monsterCounts[i] > maxCount)
            {
                maxCount = monsterCounts[i];
                maxPlayerDist = distToPlayer;
                bestAreaIndex = i;
            }
            else if (monsterCounts[i] == maxCount)
            {
                if (distToPlayer > maxPlayerDist)
                {
                    maxPlayerDist = distToPlayer;
                    bestAreaIndex = i;
                }
            }
        }
        return areaCenters[bestAreaIndex];
    }

    // 자폭
    private void ExecuteExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;

        //Debug.Log($"{gameObject.name}: 펑! 자폭했습니다.");

        // 1. 범위 안의 플레이어 감지 및 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb2D.position, explosion_range);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                player?.TakeDamage(this.pattern_damage);
                break;
            }
        }

        Destroy(gameObject);
    }

    private void UpdateWarningVisuals(float progressNormalized)
    {
        // 자폭 예고 시각화 로직 필요 시 여기에 작성
    }

    public void SetAttackingState(bool attacking)
    {
        isAttacking = attacking;
        if (isAttacking && rb2D != null) rb2D.linearVelocity = Vector2.zero;
    }
}