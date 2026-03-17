using MonsterEnum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonster : MonsterBase
{
    public override MonsterEnum.MONSTERTYPE monsterType => MonsterEnum.MONSTERTYPE.Boss;

    [Header("--- 패턴 중첩 활성화 제어 ---")]
    public bool useJump; // 점프
    public bool useFlee; // 도망
    public bool useBlink; // 점멸 
    public bool useBarricade; // 바리게이트 
    public bool useOrbit; // 보스 전용 투사체
    
    private List<GameObject> activeOrbits = new List<GameObject>();
    private float orbitTimer = 0f;
    private bool isOrbiting = false;

    [Header("--- 기획 데이터 연동 ---")]
    public float move_speed;
    public float monster_power;
    public float detect_range;
    public float pattern_cooldown;
    public float cast_time;
    public float pattern_multiply;
    public float pattern_damage;
    public float jump_height = 3.0f;
    public Transform visualChild;
    public float fleeDistance = 4.0f;
    
    [Header("---UI 변경용 이벤트 채널---")]
    [SerializeField] private FloatEventChannelSO _onBossHit;

    [Header("--- 점멸 패턴 설정 ---")]
    public float blink_detect_range = 5.0f;     // 점멸 발동 거리
    public float blink_cast_time = 1.0f;       // 점멸 예고 시간 (기 모으기)
    public float blink_speed_multiplier = 4.0f; // 점멸 이동 속도 배율
    public float blink_cooldown = 3.0f;        // 점멸 전용 쿨타임
    public LineRenderer lineRenderer;          // 점멸 경로 예고용 라인 렌더러

    [Header("--- 바리케이드 패턴 설정 ---")]
    public BarricadeSpawner barricadeSpawner;
    public float barricadeInterval = 7.0f;
    public BarricadeSpawner.SpawnLocation spawnLoc = BarricadeSpawner.SpawnLocation.Player;
    public BarricadeSpawner.BarricadeShape barShape = BarricadeSpawner.BarricadeShape.Rectangle;
    public Vector2 barSize = new Vector2(3f, 1f);
    public float barDuration = 5.0f;
    public BarricadeSpawner.InteractionType barInteraction = BarricadeSpawner.InteractionType.BlockedWithDamage;

    [Header("--- 공전 투사체 패턴 설정 ---")]
    public GameObject orbitPrefab;       // 공전할 투사체 프리팹
    public int orbitCount = 5;           // 투사체 개수
    public float orbitRadius = 2.5f;     // 공전 반지름
    public float orbitRotateSpeed = 50f; // 공전 회전 속도
    public float orbitDuration = 10f;    // 각 투사체 유지 시간
    public float orbitSpawnInterval = 0.5f; // 생성 간격
    public float orbitDamageMultiplier = 1.0f; // 데미지 배율
    public GameObject explosionEffect;    // 소멸 시 파괴 이펙트 프리팹


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
    private bool isBlinking = false; // 점멸 실행 중 여부
    private bool isWaitingBlink = false; // 점멸 예고 중 여부
    private Vector2 currentFleeTarget;

    protected override void Start()
    {
        base.Start();
        if (move_speed == 0) move_speed = base.moveSpeed;

        jumpCooldownTimer = pattern_cooldown;
        blinkCooldownTimer = blink_cooldown;
        orbitTimer = pattern_cooldown; 

        if (barricadeSpawner == null)
            barricadeSpawner = GetComponent<BarricadeSpawner>();

        // 스크립트가 시작될 때 자동으로 LineRenderer를 찾거나 초기화
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null) lineRenderer.enabled = false;

        barricadeTimer = barricadeInterval;
    }

    protected override void FixedUpdate()
    {
        if (target == null || this.hp <= 0)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // --- 패턴 로직 실행 ---

        // 공전 투사체 패턴
        if (useOrbit && !isOrbiting && orbitCooldownTimer <= 0)
        {
            StartCoroutine(OrbitPatternRoutine());
        }
        else if (orbitCooldownTimer > 0)
        {
            orbitCooldownTimer -= Time.fixedDeltaTime;
        }

        // 투사체들이 있다면 매 프레임 위치를 계산해서 회전
        if (activeOrbits.Count > 0)
        {
            UpdateOrbitPositions();
        }

        if (useBlink) HandleBlinkLogic();
        if (useJump && !isBlinking && !isWaitingBlink) HandleJumpLogic();
        if (!isJumping && !isWaitingJump && !isBlinking && !isWaitingBlink) MoveProcess();
        if (useBarricade) HandleBarricadeLogic();
    }

    #region 점멸(Blink) 로직
    private void HandleBlinkLogic()
    {
        if (isBlinking || isWaitingBlink) return;
        if (blinkCooldownTimer > 0)
        {
            blinkCooldownTimer -= Time.fixedDeltaTime;
            return;
        }

        float dist = Vector2.Distance(target.position, rb2D.position);
        if (dist <= blink_detect_range) StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        isWaitingBlink = true;
        rb2D.linearVelocity = Vector2.zero;

        Vector2 startPos = rb2D.position;
        Vector2 blinkTargetPos = target.position;

        // --- [경로 표시 추가] ---
        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, startPos);     // 시작점: 보스 위치
            lineRenderer.SetPosition(1, blinkTargetPos); // 끝점: 플레이어 위치
        }

        yield return new WaitForSeconds(blink_cast_time);

        // --- [이동 시작 시 경로 삭제] ---
        if (lineRenderer != null) lineRenderer.enabled = false;

        isWaitingBlink = false;
        isBlinking = true;

        float distance = Vector2.Distance(startPos, blinkTargetPos);
        float duration = distance / (move_speed * blink_speed_multiplier);
        float elapsed = 0f;
        bool hasDealtDamage = false;

        while (elapsed < duration && this.hp > 0)
        {
            elapsed += Time.fixedDeltaTime;
            float p = elapsed / duration;

            Vector2 nextPos = Vector2.Lerp(startPos, blinkTargetPos, p);
            rb2D.MovePosition(nextPos);

            if (!hasDealtDamage)
            {
                Collider2D hit = Physics2D.OverlapCircle(rb2D.position, 0.8f, LayerMask.GetMask("Player"));
                if (hit != null && hit.CompareTag("Player"))
                {
                    float finalDmg = pattern_damage > 0 ? pattern_damage : monster_power;
                    hit.GetComponent<IDamageable>()?.TakeDamage(finalDmg);
                    hasDealtDamage = true;
                }
            }
            yield return new WaitForFixedUpdate();
        }

        isBlinking = false;
        blinkCooldownTimer = blink_cooldown;
    }
    #endregion


    #region 공전 투사체(Orbit) 로직

    private IEnumerator OrbitPatternRoutine()
    {
        isOrbiting = true;

        // 데미지 중첩 방지
        foreach (var oldOrbit in activeOrbits)
        {
            if (oldOrbit != null)
            {
                // 폭발 이펙트 없이 조용히 삭제 (원치 않으면 실행)
                Destroy(oldOrbit);
            }
        }
        activeOrbits.Clear(); // 리스트 초기화

        // 투사체 순차 생성
        for (int i = 0; i < orbitCount; i++)
        {
            if (this.hp <= 0) break;

            GameObject orbit = Instantiate(orbitPrefab, transform.position, Quaternion.identity);

            // 데미지 전달 로직 
            var proj = orbit.GetComponent<OrbitProjectile>();
            if (proj != null)
            {
                proj.SetDamage(monster_power * orbitDamageMultiplier);
            }

            activeOrbits.Add(orbit);

            // 생성된 순서대로 소멸 타이머 가동
            StartCoroutine(DestroyOrbitAfterTime(orbit, orbitDuration));

            yield return new WaitForSeconds(orbitSpawnInterval);
        }

        orbitCooldownTimer = pattern_cooldown * 1.5f;
        isOrbiting = false;
    }

    private void UpdateOrbitPositions()
    {
        // 유효한(파괴되지 않은) 투사체만 필터링
        activeOrbits.RemoveAll(item => item == null);

        for (int i = 0; i < activeOrbits.Count; i++)
        {
            // 균등 간격 계산 + 시간에 따른 회전
            float angle = (i * (360f / orbitCount)) + (Time.time * orbitRotateSpeed);
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
            if (explosionEffect != null) Instantiate(explosionEffect, orbit.transform.position, Quaternion.identity);
            Destroy(orbit);
        }
    }
    #endregion

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBlinking && collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<IDamageable>()?.TakeDamage(pattern_damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBlinking && other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(pattern_damage);
        }
    }

    private void HandleBarricadeLogic()
    {
        barricadeTimer -= Time.fixedDeltaTime;
        if (barricadeTimer <= 0f)
        {
            if (barricadeSpawner != null)
                barricadeSpawner.SpawnBarricade(GetSpawnPosition(spawnLoc), barShape, barSize, barDuration, barInteraction, this.power);
            barricadeTimer = barricadeInterval;
        }
    }

    private Vector2 GetSpawnPosition(BarricadeSpawner.SpawnLocation loc)
    {
        if (target == null) return rb2D.position;
        switch (loc)
        {
            case BarricadeSpawner.SpawnLocation.Player: return target.position;
            case BarricadeSpawner.SpawnLocation.Boss: return rb2D.position;
            case BarricadeSpawner.SpawnLocation.Both: return (rb2D.position + (Vector2)target.position) / 2f;
            case BarricadeSpawner.SpawnLocation.None:
                Camera cam = Camera.main; if (cam == null) return rb2D.position;
                float h = cam.orthographicSize; float w = h * cam.aspect;
                return (Vector2)cam.transform.position + new Vector2(Random.Range(-w * 0.9f, w * 0.9f), Random.Range(-h * 0.9f, h * 0.9f));
            default: return rb2D.position;
        }
    }

    private void MoveProcess()
    {
        Vector2 myPos = rb2D.position;
        float dist = Vector2.Distance(target.position, myPos);
        Vector2 dir;

        if (useFlee && !isFleeing && dist < fleeDistance && fleeCooldownTimer <= 0)
        {
            currentFleeTarget = GetSmartFleePosition();
            isFleeing = true;
        }
        if (isFleeing)
        {
            dir = (currentFleeTarget - myPos).normalized;
            if (Vector2.Distance(myPos, currentFleeTarget) < 0.5f) { isFleeing = false; fleeCooldownTimer = pattern_cooldown; }
        }
        else
        {
            dir = ((Vector2)target.position - myPos).normalized;
            if (fleeCooldownTimer > 0) fleeCooldownTimer -= Time.fixedDeltaTime;
        }
        rb2D.linearVelocity = dir * (isFleeing ? move_speed * pattern_multiply : move_speed);
    }
    public override void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
        _onBossHit.OnStartEvent(damage);
    }

    private void HandleJumpLogic()
    {
        if (isJumping || isWaitingJump) return;
        if (jumpCooldownTimer > 0) { jumpCooldownTimer -= Time.fixedDeltaTime; return; }
        if (Vector2.Distance(target.position, rb2D.position) <= detect_range) StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        isWaitingJump = true; rb2D.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(cast_time);
        isWaitingJump = false; isJumping = true;
        Vector2 start = rb2D.position; Vector2 dest = target.position;

        float dur = Vector2.Distance(start, dest) / (move_speed * pattern_multiply);
        float elp = 0f;

        while (elp < dur && this.hp > 0)
        {
            elp += Time.fixedDeltaTime; float p = elp / dur;

            rb2D.MovePosition(Vector2.Lerp(start, dest, p));
            if (visualChild != null) visualChild.localPosition = new Vector2(0, Mathf.Sin(p * Mathf.PI) * jump_height);

            yield return new WaitForFixedUpdate();
        }
        if (visualChild != null) visualChild.localPosition = Vector2.zero;
        isJumping = false; jumpCooldownTimer = pattern_cooldown;
    }

    private Vector2 GetSmartFleePosition()
    {
        Camera cam = Camera.main; if (cam == null) return rb2D.position;

        float h = 2f * cam.orthographicSize; float w = h * cam.aspect;

        Vector2 camP = (Vector2)cam.transform.position;
        Vector2[] areas = new Vector2[6]; int[] counts = new int[6];

        for (int i = 0; i < 6; i++)
        {
            float x = (i < 3) ? camP.x - (w / 4f) : camP.x + (w / 4f);
            float y = camP.y + (h / 3f) * (1 - (i % 3));
            areas[i] = new Vector2(x, y);
        }

        foreach (var m in GameObject.FindGameObjectsWithTag("Monster"))
        {
            if (m == gameObject) continue;
            float minD = float.MaxValue; int cA = -1;
            for (int j = 0; j < 6; j++)
            {
                float d = Vector2.Distance(m.transform.position, areas[j]);
                if (d < minD) { minD = d; cA = j; }
            }
            if (cA != -1) counts[cA]++;
        }

        int best = 0; int maxC = -1; float maxD = -1f;

        for (int i = 0; i < 6; i++)
        {
            float dP = Vector2.Distance(areas[i], target.position);

            if (counts[i] > maxC || (counts[i] == maxC && dP > maxD))
            {
                maxC = counts[i]; maxD = dP; best = i;
            }
        }
        return areas[best];
    }
}