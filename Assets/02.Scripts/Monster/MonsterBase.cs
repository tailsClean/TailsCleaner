using UnityEngine;

public enum MonsterType { Normal, SpecialPattern, Elite, Boss }

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : MonoBehaviour
{
    [Header("--- 몬스터 기본 정보 ---")]
    public int monsterId;
    public string monsterName;
    public MonsterType monsterType;

    [Header("--- 몬스터 스탯 ---")]
    public float hp = 1.0f;        
    public float mass = 1.0f;
    public float moveSpeed = 1.0f;
    public float stoppingDistance = 0.1f;

    [Header("--- 3D 좌표 설정 ---")]
    public float fixedWorldHeightY = 0f;
    public Transform target;

    protected Rigidbody2D rb2D;

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

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

    protected virtual void MoveToTarget()
    {
        // 3D 위치 가져오기
        Vector3 myPos3D = transform.position;
        Vector3 targetPos3D = target.position;

        // (X, Z) 벡터 계산
        Vector3 diff = new Vector3(targetPos3D.x - myPos3D.x, 0, targetPos3D.z - myPos3D.z);
        float distance = diff.magnitude;

        if (distance <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        // 방향 벡터 생성 및 다음 위치 계산
        Vector3 dir = diff.normalized;
        Vector3 nextPos3D = myPos3D + dir * moveSpeed * Time.fixedDeltaTime;

        // 3D 좌표를 2D 물리 좌표(X, Y)로 강제 매핑 (Z -> Y)
        rb2D.position = new Vector2(nextPos3D.x, nextPos3D.z);

        // 실제 3D 오브젝트 위치 업데이트
        transform.position = new Vector3(nextPos3D.x, fixedWorldHeightY, nextPos3D.z);

        // 디버그 확인: 여기서 Z값이 변하는지 확인
        //Debug.Log($"Target Z: {targetPos3D.z:F2} | My Z: {transform.position.z:F2} | Dir: {dir}");
    }

    public void SyncTransformToPhysics()
    {
        if (rb2D != null)
            rb2D.position = new Vector2(transform.position.x, transform.position.z);
    }
}