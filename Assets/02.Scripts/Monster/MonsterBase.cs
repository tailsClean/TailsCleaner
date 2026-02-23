using UnityEngine;
using MonsterEnum;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class MonsterBase : MonoBehaviour, IDamageable
{
    [Header("--- 환경 설정 ---")]
    public Transform target;
    public float stoppingDistance = 0.1f;

    [Header("--- 몬스터 정체성 ---")]
    public abstract MonsterType monsterType { get; }

    [Header("--- 기준 스탯 ---")]
    public float hp = 1.0f;
    public float power = 1.0f;
    public float moveSpeed = 1.0f;
    public float hitBox = 1.0f;
    public float mass = 1.0f;
    public float KBResist = 1.0f;

    [Header("--- Drop Items ---")]
    [SerializeField] private GameObject TestItem;

    [Header("--- 공격 설정 ---")]
    public float damageCooldown = 1.0f; // 공격 간격
    private float lastAttackTime;       // 마지막 공격 시간

    protected Rigidbody2D rb2D;
    protected bool isAttacking = false; // 패턴 중 이동 정지용

    protected virtual void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();

        rb2D.bodyType = RigidbodyType2D.Kinematic;
        rb2D.gravityScale = 0f;
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb2D.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    protected virtual void Start()
    {
        // 초기 위치 동기화
        if (rb2D != null) rb2D.position = transform.position;
    }

    protected virtual void FixedUpdate()
    {
        if (target == null || isAttacking) return;
        MoveToTarget();
    }

    protected virtual void MoveToTarget()
    {
        StraightChase();
    }

    // 2D 전용 직선 추격 (Rigidbody 물리 이동)
    protected void StraightChase()
    {
        Vector2 myPos = rb2D.position;
        Vector2 targetPos = target.position;
        Vector2 diff = targetPos - myPos;

        if (diff.magnitude <= stoppingDistance)
        {
            rb2D.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = diff.normalized;
        Vector2 nextPos = myPos + dir * moveSpeed * Time.fixedDeltaTime;
        rb2D.MovePosition(nextPos);
    }

    // 자식 클래스(Special, Boss)에서 사용할 좌표 적용 함수
    protected void ApplyPosition(Vector2 nextPos)
    {
        rb2D.MovePosition(nextPos);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        if (hp <= 0) Die();
    }

    protected virtual void OnTriggerStay2D(Collider2D other)
    {
        // 닿은 대상이 target인지 확인
        if (target != null && other.gameObject == target.gameObject)
        {
            // 공격 주기가 되었는지 확인
            if (Time.time >= lastAttackTime + damageCooldown)
            {
                // 플레이어에게 데미지 전달 시도
                IDamageable player = other.gameObject.GetComponent<IDamageable>();

                if (player != null)
                {
                    player.TakeDamage(this.power); // 플레이어의 함수 호출
                    lastAttackTime = Time.time;    // 쿨타임 초기화

                    // 확인을 위한 로그
                    //Debug.Log($"{gameObject.name}가 트리거로 플레이어에게 데미지를 입혔음.");
                }
            }
        }
    }

    protected virtual void Die()
    {
        if (TestItem != null)
        {
            Instantiate(TestItem, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}