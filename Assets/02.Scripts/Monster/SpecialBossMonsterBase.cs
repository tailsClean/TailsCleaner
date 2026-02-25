using UnityEngine;
using System.Linq; // 밀집도 계산
using MonsterEnum;

public abstract class SpecialBossMonsterBase : MonsterBase
{
    [Header("--- 이동 특수 패턴 설정 ---")]
    public MonsterMove moveType;

    [Header("--- 패턴 상세 수치 ---")]
    public float patternAmplitude = 5.0f; // 지그재그 진폭
    public float patternFrequency = 5.0f; // 주기
    public float fleeDistance = 4.0f;     // 도망 발동 거리

    [Header("--- 추가 설정 ---")]
    public float stateDuration = 3.0f;    // 추격/도망 전환 주기 
    public float idleDuration = 1.0f;     // 점프 후 대기 시간 

    protected float patternTimer = 0f;    // 지그재그 박자 맞추기용
    protected float stateTimer = 0f;      // 상태 전환용 타이머
    protected bool isWaiting = false;     // 대기 모션 여부
    protected bool isFleeingState = false; // 현재 도망 상태인지 여부

    [Header("--- 점프 상세 설정 ---")]
    public float jumpCooldown = 3.0f;     // 점프 간격
    public float jumpDuration = 0.8f;     // 공중 체류 시간
    
    
    private bool isJumping = false;
    private Vector2 jumpStartPos;
    private Vector2 jumpTargetPos;
    private float jumpProgress = 0f;

    protected override void MoveToTarget()
    {
        patternTimer += Time.fixedDeltaTime;
        stateTimer += Time.fixedDeltaTime;

        // 점프 패턴인 경우 대기 로직
        if (moveType == MonsterMove.Jump && isWaiting)
        {
            if (stateTimer >= idleDuration)
            {
                isWaiting = false;
                stateTimer = 0;
            }
            rb2D.linearVelocity = Vector2.zero; // 대기 중 정지
            return;
        }

        // 이동 타입별 분기
        switch (moveType)
        {
            case MonsterMove.StraightChase:
                StraightChase();
                break;
            case MonsterMove.Zigzag:
                ZigzagMove();
                break;
            case MonsterMove.Jump:
                JumpMove();
                break;
            case MonsterMove.Flee:
                FleeMove();
                break;
            default:
                StraightChase();
                break;
        }
    }

    // 지그재그
    protected void ZigzagMove()
    {
        Vector2 myPos = rb2D.position;
        Vector2 dirToTarget = ((Vector2)target.position - myPos).normalized;

        // 수직 벡터 계산
        Vector2 sideDir = new Vector2(-dirToTarget.y, dirToTarget.x);
        float wave = Mathf.Sin(patternTimer * patternFrequency) * patternAmplitude;

        Vector2 finalDir = (dirToTarget + sideDir * wave).normalized;
        rb2D.MovePosition(myPos + finalDir * moveSpeed * Time.fixedDeltaTime);
    }

    // 점프
    protected void JumpMove()
    {
        // 1. 점프 중 상태 (플레이어 위치로 이동)
        if (isJumping)
        {
            jumpProgress += Time.fixedDeltaTime / jumpDuration;

            if (jumpProgress >= 1f)
            {
                // 착지 완료
                rb2D.MovePosition(jumpTargetPos);
                isJumping = false;
                isWaiting = true;  // 착지 후 대기 상태 진입
                stateTimer = 0;    // 대기(Idle) 타이머 시작
            }
            else
            {
                // 플레이어 위치로 직선 이동
                rb2D.MovePosition(Vector2.Lerp(jumpStartPos, jumpTargetPos, jumpProgress));
            }
            return;
        }

        // 착지 상태 로직
        if (!isWaiting) // 착지 후 대기(idleDuration)가 끝난 상태라면
        {
            // 다음 점프가 가능해질 때까지 기다림 (추격하지 않음)
            rb2D.linearVelocity = Vector2.zero;

            if (stateTimer >= jumpCooldown)
            {
                // 점프 가능 시점이 되면 점프 실행
                isJumping = true;
                jumpProgress = 0f;
                jumpStartPos = rb2D.position;
                jumpTargetPos = target.position; // 플레이어 위치 저장
                stateTimer = 0;
            }
        }
        else
        {
            // 이 부분은 MoveToTarget 상단부의 isWaiting 체크에서 처리됨 (정지 상태)
            rb2D.linearVelocity = Vector2.zero;
        }
    }

    // 도망
    protected void FleeMove()
    {
        // 일정 시간마다 상태 전환 (추격 <-> 도망)
        if (stateTimer >= stateDuration)
        {
            isFleeingState = !isFleeingState;
            stateTimer = 0;
        }

        if (!isFleeingState)
        {
            // 직선 추격
            StraightChase();
        }
        else
        {
            // 몬스터 밀도가 높은 위치로 도망
            Vector2 targetPos = GetHighDensityPosition();
            Vector2 myPos = rb2D.position;
            Vector2 dir = (targetPos - myPos).normalized;

            rb2D.MovePosition(myPos + dir * moveSpeed * Time.fixedDeltaTime);
        }
    }

    // 몬스터 밀집 지역 계산
    private Vector2 GetHighDensityPosition()
    {
        // Monster 태그를 가진 객체들을 탐색
        var neighbors = GameObject.FindGameObjectsWithTag("Monster")
            .Where(m => m != this.gameObject)
            .Select(m => (Vector2)m.transform.position)
            .ToList();

        if (neighbors.Count == 0) return rb2D.position; // 주변에 없으면 제자리

        // 평균 위치 계산
        Vector2 center = Vector2.zero;
        foreach (var pos in neighbors) center += pos;
        return center / neighbors.Count;
    }
}