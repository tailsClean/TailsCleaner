using UnityEngine;
using MonsterEnum;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    [Header("--- 환경 설정 ---")]
    [Tooltip("체크하면 3D(X,Z축 사용), 체크 해제하면 2D(X,Y축 사용)")]
    public bool is3DMode = true;
    public Transform target;
    public float stoppingDistance = 0.1f; // 타겟 정지 거리

    [Header("--- 몬스터 정체성 ---")]
    public MonsterType monsterType;
    public MonsterMove moveType;

    [Header("--- 기준 몬스터 ---")]
    public float hp = 1.0f;
    public float power = 1.0f;
    public float moveSpeed = 1.0f;
    public float hitBox = 1.0f;
    public float mass = 1.0f;
    public float KBResist = 1.0f;

    [Header("--- 좌표 설정 ---")]
    public float fixedWorldHeightY = 0f; // 3D일 때만 사용되는 고정 높이

    [Header("--- Drop Items ---")]
    [SerializeField] private GameObject TestItem; // TestItem 프리팹 할당


    protected Rigidbody2D rb2D;

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        // 물리 설정 초기화
        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.gravityScale = 0f;
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    protected virtual void Start()
    {
        SyncTransformToPhysics();
    }

    protected virtual void FixedUpdate()
    {
        if (target == null) return;
        MoveToTarget();
    }

    // 타겟을 따라가는 로직
    protected virtual void MoveToTarget()
    {
        Vector3 myPos = transform.position;
        Vector3 targetPos = target.position;

        // 현재 환경에 맞는 '상하/앞뒤' 좌표값 추출
        // 3D라면 Z값을, 2D라면 Y값을 가져옴.
        float myVertical = is3DMode ? myPos.z : myPos.y;
        float targetVertical = is3DMode ? targetPos.z : targetPos.y;

        // 방향 계산 (2D 벡터로 통합 계산)
        Vector2 diff = new Vector2(targetPos.x - myPos.x, targetVertical - myVertical);
        float distance = diff.magnitude;

        if (distance <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // 방향 벡터 생성 및 다음 위치 계산
        Vector2 dir = diff.normalized;
        Vector2 nextStep = dir * moveSpeed * Time.fixedDeltaTime;

        // 최종 좌표 확정
        float finalX = myPos.x + nextStep.x;
        float finalVertical = myVertical + nextStep.y;

        // 물리 엔진 및 실제 Transform 업데이트
        if (is3DMode)
        {
            // [3D 모드] 연산된 세로값을 Z에 할당
            rb2D.position = new Vector2(finalX, finalVertical);
            transform.position = new Vector3(finalX, fixedWorldHeightY, finalVertical);
        }
        else
        {
            // [2D 모드] 연산된 세로값을 Y에 할당
            rb2D.position = new Vector2(finalX, finalVertical);
            transform.position = new Vector3(finalX, finalVertical, myPos.z); // Z는 유지
        }
    }

    // Rigidbody2D 위치 동기화 로직
    public void SyncTransformToPhysics()
    {
        if (rb2D == null) return;

        // 초기 동기화도 모드에 따라 다르게 처리
        if (is3DMode)
            rb2D.position = new Vector2(transform.position.x, transform.position.z);
        else
            rb2D.position = new Vector2(transform.position.x, transform.position.y);
    }

    // 피격 및 판정 로직
    public void TakeDamage(float damage)
    {
        hp -= damage;
        //Debug.Log($"{monsterName} 남은 체력: {hp}");
        if (hp <= 0) Die();
    }

    // 몬스터 사망 로직
    protected virtual void Die()
    {
        // 몬스터 사망 로그 
        //Debug.Log($"<color=red><b>[사망]</b></color> {monsterName}(ID: {monsterId})가 처치되었습니다!");

        // 드롭 아이템 생성 로직
        if (TestItem != null)
        {
            Instantiate(TestItem, transform.position, Quaternion.identity);
        }

        // 확인을 위한 객체 삭제
        Destroy(gameObject);
    }
}