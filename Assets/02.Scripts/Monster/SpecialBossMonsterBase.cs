using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SpecialBossMonsterBase : MonsterBase
{
    protected static List<SpecialBossMonsterBase> activeMonsters = new List<SpecialBossMonsterBase>();

    protected enum MonsterState { MOVE, PATTERN }
    protected MonsterState currentState = MonsterState.MOVE;


    [Header("--- 데이터 테이블 연동 ---")]
    public int pattern_group_id;

    [Header("--- monster_table 연동  ---")]
    public float detect_range;             // 플레이어를 감지하거나 패턴을 발동하는 기준 거리

    [Header("--- pattern_table 연동 ---")]
    public float pattern_cooldown;         // 패턴 대기 시간
    public float cast_time;                // 패턴 발동 전 대기 시간
    public float pattern_multiply;         // 패턴 중 가속 배율
    public float explosion_range;          // 자폭/광역 공격의 물리적 타격 반경
    public float damage_multiply;          // 패턴 성공 시 플레이어에게 주는 데미지 배율
    public float zigzag_width;             // 좌우 이동 폭
    public float patternFrequency = 5.0f;  // 지그재그 주기
    public float type_power_multiply = 1.0f;

    [Header("--- 이동 특수 패턴 설정 ---")]
    public MONSTERMOVE moveType;           // 이 몬스터가 어떤 이동 패턴을 쓸지 결정

    [Header("--- 점프 전용 상세 설정 ---")]
    public float jump_height = 2.0f;       // 시각적 높이
    public Transform visualChild;

    // --- 내부 제어용 변수 ---
    protected float patternTimer = 0f;     // 지그재그용 타이머
    protected float stateTimer = 0f;       // 쿨타임 및 시전 대기 타이머
    protected bool isWaiting = false;      // 시전 대기 중(cast_time 체크용)
    protected bool isJumping = false;      // 점프 여부
    private bool hasHitTargetInCurrentJump = false; // 중복 데미지 방지용
    private Vector2 jumpStartPos;          // 점프 시작 지점
    private Vector2 jumpTargetPos;         // 점프 착지 시점
    private float jumpProgress = 0f;       // 점프 진행도 (0~1)

    protected bool isFleeingState = false; // 도망 여부
    protected bool isWaitingFlee = false;  // 도망 대기 여부
    private Vector2 currentFleeTargetPos;  // 선택된 도망 목표 지점

    public bool isSuicideUnit = false;     // 자폭 여부
    private bool hasExploded = false;      // 이미 터졌는지 체크
    private float currentCastTimer;        // 실시간 자폭 대기 타이머

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        if (target == null) return;
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
            if (!isAttacking)
            {
                MoveToTarget();
            }
        }
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        if (!activeMonsters.Contains(this))
            activeMonsters.Add(this);

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
    }

    protected override void MoveToTarget()
    {
        patternTimer += Time.fixedDeltaTime;
        stateTimer += Time.fixedDeltaTime;

        if (isSuicideUnit && !hasExploded)
        {
            // 부모의 moveSpeed 사용
            float suicideSpeed = this.moveSpeed * pattern_multiply;
            Vector2 dir = ((Vector2)target.position - rb2D.position).normalized;
            float dist = Vector2.Distance(target.position, rb2D.position);

            if (dist > 0.1f)
                rb2D.linearVelocity = dir * suicideSpeed;
            else
                rb2D.linearVelocity = Vector2.zero;
            return;
        }

        switch (moveType)
        {
            case MONSTERMOVE.StraightChase: StraightChase(); break;
            case MONSTERMOVE.Zigzag: ZigzagMove(); break;
            case MONSTERMOVE.Jump: JumpMove(); break;
            case MONSTERMOVE.Flee: FleeMove(); break;
            default: StraightChase(); break;
        }
    }

    protected void ZigzagMove()
    {
        Vector2 myPos = rb2D.position;
        Vector2 targetPos = (Vector2)target.position;
        Vector2 toTarget = targetPos - myPos;
        float dist = toTarget.magnitude;

        Vector2 baselineDir = (dist > 0.1f) ? toTarget.normalized : rb2D.linearVelocity.normalized;
        Vector2 sideDir = new Vector2(-baselineDir.y, baselineDir.x);

        float sideOffset = Mathf.Sin(patternTimer * patternFrequency) * zigzag_width;
        float damping = Mathf.Clamp01((dist - 0.2f) / 0.8f);

        float zigzagSpeed = this.moveSpeed * pattern_multiply;
        Vector2 movement = (baselineDir * zigzagSpeed) + (sideDir * sideOffset * patternFrequency * damping);

        if (dist < 0.1f)
            rb2D.linearVelocity = Vector2.zero;
        else
            rb2D.linearVelocity = movement;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (isSuicideUnit) return;

        if (collision.CompareTag("Player"))
        {
            IDamageable player = collision.GetComponent<IDamageable>();
            if (player == null) return;

            if (moveType == MONSTERMOVE.Zigzag)
            {
                player.TakeDamage(this.power);
                return;
            }

            if (moveType == MONSTERMOVE.Jump && isJumping && !hasHitTargetInCurrentJump)
            {
                float finalJumpDamage = this.power * this.damage_multiply;
                player.TakeDamage(finalJumpDamage);
                hasHitTargetInCurrentJump = true;
                return;
            }

            player.TakeDamage(this.power);
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
            stateTimer = 0;
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        if (isWaiting)
        {
            if (stateTimer >= cast_time)
            {
                isWaiting = false;
                isJumping = true;
                hasHitTargetInCurrentJump = false;
                jumpStartPos = rb2D.position;
                jumpTargetPos = target.position;
                jumpProgress = 0f;
                stateTimer = 0;
            }
            return;
        }

        if (isJumping)
        {
            float actualSpeed = this.moveSpeed * pattern_multiply;
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
            else
            {
                rb2D.linearVelocity = Vector2.zero;
                isWaitingFlee = true;
                stateTimer = 0;
                return;
            }
        }

        if (isWaitingFlee)
        {
            rb2D.linearVelocity = Vector2.zero;
            if (stateTimer >= cast_time)
            {
                isWaitingFlee = false;
                isFleeingState = true;
                stateTimer = 0;
                currentFleeTargetPos = GetSmartFleePosition();
            }
            return;
        }

        if (isFleeingState)
        {
            Vector2 dir = (currentFleeTargetPos - rb2D.position).normalized;
            float distToTarget = Vector2.Distance(rb2D.position, currentFleeTargetPos);

            rb2D.linearVelocity = dir * (this.moveSpeed * pattern_multiply);

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
        stateTimer = 0;
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

        foreach (var mObj in allMonsters)
        {
            if (mObj == this.gameObject) continue;

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

        // 부모 power * 패턴 배율
        float finalDamage = this.power * this.damage_multiply;

        Collider2D[] hits = Physics2D.OverlapCircleAll(rb2D.position, explosion_range);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                IDamageable player = hit.GetComponent<IDamageable>();
                if (player != null)
                {
                    player.TakeDamage(finalDamage);
                }
                break;
            }
        }
        ObjectPoolManager.Instance.ReturnObject(this);
    }

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
            rb2D.linearVelocity = dir * this.moveSpeed;
        }
        else
        {
            rb2D.linearVelocity = Vector2.zero;
        }
    }
}