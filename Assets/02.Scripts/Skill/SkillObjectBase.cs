using System.Collections.Generic;
using UnityEngine;

public class SkillObjectBase : MonoBehaviour
{
    protected SkillStat _runtimeBaseStat;         // 런타임 기본 스탯
    protected SkillStat _runtimeCommonStat;       // 런타임 공용 스탯
    protected SkillStat _runtimeUpgradeStat;      // 런타임 업그레이드 스탯
    protected SkillStat _runtimePassiveMulStat;   // 런타임 패시브 배율 합 , 임플란트 누적
    protected SkillStat _runtimeFinalStat;        // 최종 스탯

    protected ActiveSkill _skill;                 // 액티브 스킬 (스탯 재계산용)
    protected Rigidbody2D _rigidbody;             // 속도용

    protected Vector2 _dir;                       // 방향
    protected float _createTime;                  // 생성 시간
    protected float _lastDurationTickTime;        // 최근 지속시간 틱 시간
    protected bool _expired = false;              // 수명 만료 상태 (중복 방지, 추후 연출용)

    protected List<PassiveModifier> _passiveModifiers;    // 패시브 모디파이어

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    protected void Init(ActiveSkill owner, Vector2 dir)
    {
        _skill = owner;
        _dir = dir;

        // 스킬 스탯 스냅샷
        _runtimeBaseStat = owner.BaseStat.Clone();
        _runtimeCommonStat = owner.CommonStat.Clone();
        _runtimeUpgradeStat = owner.UpgradeStat.Clone();
        _runtimePassiveMulStat = owner.PassiveMulStat.Clone();
        _runtimeFinalStat = owner.FinalStat.Clone();

        // 패시브 모디파이어 캐싱
        _passiveModifiers = new List<PassiveModifier>(owner.PassiveModifiers);

        _createTime = Time.time;
        _expired = false;

        OnInit();

        // 물리 적용
        ApplyPhysics();
    }

    protected virtual void Update()
    {
        // 수명 체크
        if (_expired == false && Time.time >= _createTime + _runtimeFinalStat.Duration)
            ExpireObject();

        // 스킬 지속 시간 틱 체크 (스노우볼링)
        if (_runtimeFinalStat.DurationTickInterval <= 0f) return;
        if (Time.time >= _lastDurationTickTime + _runtimeFinalStat.DurationTickInterval)
        {
            OnDurationTick();
            _lastDurationTickTime = Time.time;
        }
    }

    // 스탯 재계산
    protected void CalculateStat()
    {
        _runtimeFinalStat = _skill.GetFinalStat(
            _runtimeBaseStat,
            _runtimeCommonStat,
            _runtimeUpgradeStat,
            _runtimePassiveMulStat);
    }

    // 물리 적용 (속도, 크기)
    protected void ApplyPhysics()
    {
        if (_rigidbody != null)
        {
            _rigidbody.linearVelocity = _dir * _runtimeFinalStat.ProjectileSpeed;
        }

        transform.localScale = Vector3.one * _runtimeFinalStat.Size;
    }

    // 방향 설정
    protected void SetDirection(Vector2 newDir)
    {
        _dir = newDir;
        // 방향 변경 후 물리 재적용
        ApplyPhysics();
    }


    // 수명 만료
    protected void ExpireObject()
    {        
        if (_expired == true) return;
        _expired = true;

        OnExpire();

        Destroy(gameObject);
        // 풀링 반환으로 변경
    }


    // 생성 직후
    // 전용 모디파이어 전처리 -> 패시브 초기화
    protected virtual void OnInit()
    {
        // 자식 전용 모디파이어 처리 후 재계산 여부
        bool recalcul = OnCustomInit();

        // 패시브 모디파이어 순회
        foreach (var passive in _passiveModifiers)
        {
            // 생성 직후 로직 있으면 실행
            if (passive.OnProjectileInit(_runtimeBaseStat, _runtimeFinalStat))
                recalcul = true;
        }

        // 스탯 재계산
        if (recalcul)
            CalculateStat();
    }

    // 전용 모디파이어 처리 후 재계산 여부
    protected virtual bool OnCustomInit()
    {
        return false;
    }

    // 스킬 지속시간 틱
    // 스노우볼링 0.5초마다 패시브 스탯 배율 추가
    private void OnDurationTick()
    {
        foreach (var passive in _passiveModifiers)
            passive.OnDurationTick(_runtimePassiveMulStat);

        // 스탯 재계산
        CalculateStat();
    }

    // 수명 만료시 호출
    // 파괴될 때 추가 로직, 연출 후 파괴되게
    protected virtual void OnExpire() { }   
}
