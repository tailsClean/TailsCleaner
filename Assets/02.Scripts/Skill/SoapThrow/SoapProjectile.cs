using UnityEngine;

public class SoapProjectile : MonoBehaviour
{
    private SkillStat _stat;                    // 스탯
    private Vector2 _dir;                       // 방향
    private SoapModifierData _modifierData;     // 모디파이어
    private Rigidbody2D _rigidbody;             // 리지드바디
    private Collider2D _collider;               // 콜라이더
    private float _createTime;                  // 생성 시간
    private int _currentPierceCount = 0;        // 현재 관통 횟수

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponentInChildren<Collider2D>();
    }

    public void Init(SkillStat stat, Vector2 dir, SoapModifierData modifierData)
    {
        _stat = stat;
        _dir = dir;
        _modifierData = modifierData;
        _createTime = Time.time;

        // 속도 설정
        if(_rigidbody != null) _rigidbody.linearVelocity = _dir * _stat.ProjectileSpeed;

        // 크기 설정
        transform.localScale = Vector3.one * _stat.Size;
    }

    private void Update()
    {
        // 수명 시간 체크
        if (Time.time >= _createTime + _stat.Duration)
        {
            DestroyProjectile();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적 충돌 처리 해야함

        // 관통 처리
        _currentPierceCount++;

        // 관통 횟수 초과 시 파괴
        //if (_currentPierceCount > _stat.PierceCount)
        //{
        //    DestroyProjectile();
        //}
        //else if (_modifierData.Retracking == true)
        //{
        //    // 감나빗! 모디파이어 관통 후 재추적
        //    RetargetEnemy();
        //}
    }


    // 관통 후 새로운 적 재추적
    private void RetargetEnemy()
    {
        Debug.Log("[SoapProjectile] 재추적 로직 실행");
    }


    // 투사체 파괴
    private void DestroyProjectile()
    {
        Destroy(gameObject);

        // 나중에 풀 반환
    }
}
