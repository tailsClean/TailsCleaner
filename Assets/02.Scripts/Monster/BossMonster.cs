using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster : MonsterBase
{
    public override MonsterEnum.MonsterType monsterType => MonsterEnum.MonsterType.Boss;

    [Header("--- 패턴 중첩 활성화 제어 ---")]
    public bool useZigzag;
    public bool useJump;
    public bool isSuicideUnit;
    public bool useFlee;

    [Header("--- 기획 데이터 연동 ---")]
    public float move_speed;
    public float monster_power;
    public float detect_range;
    public float pattern_cooldown;
    public float cast_time;
    public float pattern_multiply;
    public float explosion_range;
    public float pattern_damage;
    public float zigzag_width = 2.0f;
    public float patternFrequency = 5.0f;
    public float jump_height = 3.0f;
    public Transform visualChild;
    public float fleeDistance = 4.0f;

    // 내부 제어 변수
    private float zigzagTimer = 0f;
    private float jumpCooldownTimer = 0f;
    private float suicideCastTimer = 0f;
    private float fleeCooldownTimer = 0f;

    private bool isJumping = false;
    private bool isWaitingJump = false; // 점프 전 기모으기 상태
    private bool hasExploded = false;
    private bool isFleeing = false;
    private Vector2 currentFleeTarget;

    protected override void Start()
    {
        base.Start();
        if (move_speed == 0) move_speed = base.moveSpeed;
        suicideCastTimer = cast_time;
        jumpCooldownTimer = pattern_cooldown;
    }

    protected override void FixedUpdate()
    {
        if (target == null || hasExploded || this.hp <= 0)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // 자폭 로직 
        if (isSuicideUnit) HandleSuicideLogic();

        // 점프 로직 우선 체크 (점프 준비 중이거나 점프 중이면 이동 로직을 완전히 건너뜀)
        if (useJump && !isSuicideUnit)
        {
            HandleJumpLogic();
        }

        // 이동 로직 (점프 관련 상태가 아닐 때만 실행)
        if (!isJumping && !isWaitingJump)
        {
            MoveProcess();
        }
    }

    private void MoveProcess()
    {
        Vector2 myPos = rb2D.position;
        float distToPlayer = Vector2.Distance(target.position, myPos);
        Vector2 baseMoveDir;

        // 도망 로직
        if (useFlee && !isFleeing && distToPlayer < fleeDistance && fleeCooldownTimer <= 0)
        {
            currentFleeTarget = GetSmartFleePosition();
            isFleeing = true;
        }

        if (isFleeing)
        {
            baseMoveDir = (currentFleeTarget - myPos).normalized;
            if (Vector2.Distance(myPos, currentFleeTarget) < 0.5f)
            {
                isFleeing = false;
                fleeCooldownTimer = pattern_cooldown;
            }
        }
        else
        {
            baseMoveDir = ((Vector2)target.position - myPos).normalized;
            if (fleeCooldownTimer > 0) fleeCooldownTimer -= Time.fixedDeltaTime;
        }

        // 지그재그 적용
        if (useZigzag)
        {
            zigzagTimer += Time.fixedDeltaTime;
            Vector2 sideDir = new Vector2(-baseMoveDir.y, baseMoveDir.x);
            float sideOffset = Mathf.Sin(zigzagTimer * patternFrequency) * zigzag_width;
            float damping = isFleeing ? 1.0f : Mathf.Clamp01((distToPlayer - 0.2f) / 0.8f);

            float speed = isFleeing ? move_speed * pattern_multiply : move_speed;
            rb2D.linearVelocity = (baseMoveDir * speed) + (sideDir * sideOffset * patternFrequency * damping);
        }
        else
        {
            float speed = isFleeing ? move_speed * pattern_multiply : move_speed;
            rb2D.linearVelocity = baseMoveDir * speed;
        }
    }

    private void HandleJumpLogic()
    {
        // 이미 점프 프로세스 중이면 중복 실행 방지
        if (isJumping || isWaitingJump) return;

        if (jumpCooldownTimer > 0)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
            return;
        }

        float dist = Vector2.Distance(target.position, rb2D.position);
        // 사거리 안에 들어오면 점프 시퀀스 시작
        if (dist <= detect_range)
        {
            StartCoroutine(JumpRoutine());
        }
    }

    private IEnumerator JumpRoutine()
    {
        isWaitingJump = true;
        rb2D.linearVelocity = Vector2.zero; // 지그재그 속도 즉시 제거

        // 점프 전 예비 동작 (기모으기 시간)
        yield return new WaitForSeconds(cast_time);

        isWaitingJump = false;
        isJumping = true;

        Vector2 startPos = rb2D.position;
        Vector2 targetPos = target.position;
        float totalDist = Vector2.Distance(startPos, targetPos);
        float duration = totalDist / (move_speed * pattern_multiply);
        float elapsed = 0f;

        while (elapsed < duration && this.hp > 0)
        {
            elapsed += Time.fixedDeltaTime;
            float p = elapsed / duration;

            // 점프 중에는 물리 엔진(Velocity)이 아닌 Position 직접 이동으로 지그재그 간섭 원천 차단
            rb2D.MovePosition(Vector2.Lerp(startPos, targetPos, p));

            if (visualChild != null)
            {
                float h = Mathf.Sin(p * Mathf.PI) * jump_height;
                visualChild.localPosition = new Vector2(0, h);
            }
            yield return new WaitForFixedUpdate();
        }

        if (visualChild != null) visualChild.localPosition = Vector2.zero;
        isJumping = false;
        jumpCooldownTimer = pattern_cooldown; // 착지 후 쿨타임 시작
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
            float minDist = float.MaxValue; int closestArea = -1;
            for (int j = 0; j < 6; j++)
            {
                float d = Vector2.Distance(mObj.transform.position, areaCenters[j]);
                if (d < minDist) { minDist = d; closestArea = j; }
            }
            if (closestArea != -1) monsterCounts[closestArea]++;
        }
        int bestAreaIndex = 0; int maxCount = -1; float maxPlayerDist = -1f;
        for (int i = 0; i < 6; i++)
        {
            float dToPlayer = Vector2.Distance(areaCenters[i], target.position);
            if (monsterCounts[i] > maxCount || (monsterCounts[i] == maxCount && dToPlayer > maxPlayerDist))
            {
                maxCount = monsterCounts[i]; maxPlayerDist = dToPlayer; bestAreaIndex = i;
            }
        }
        return areaCenters[bestAreaIndex];
    }

    private void HandleSuicideLogic()
    {
        suicideCastTimer -= Time.fixedDeltaTime;
        if (suicideCastTimer <= 0) ExecuteExplosion();
    }

    private void ExecuteExplosion()
    {
        if (hasExploded) return;
        hasExploded = true;
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb2D.position, explosion_range);
        foreach (var hit in hits)
            if (hit.CompareTag("Player")) hit.GetComponent<IDamageable>()?.TakeDamage(pattern_damage);
        if (ObjectPoolManager.Instance != null) ObjectPoolManager.Instance.ReturnObject(this);
        else Destroy(gameObject);
    }
}