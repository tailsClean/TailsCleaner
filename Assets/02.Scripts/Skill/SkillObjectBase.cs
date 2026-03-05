using System.Collections.Generic;
using UnityEngine;

public class SkillObjectBase : MonoBehaviour
{
    protected SkillStat _runtimeBaseStat = new();       // 런타임 기본 스탯
    protected SkillStat _runtimeCommonStat = new();     // 런타임 공용 스탯
    protected SkillStat _runtimeUpgradeStat = new();    // 런타임 업그레이드 스탯
    protected SkillStat _runtimePassiveMulStat = new(); // 런타임 패시브 배율 합 , 임플란트 누적
    protected SkillStat _runtimeFinalStat = new();      // 최종 스탯
    private SkillStat _calcBuffer = new();              // 스탯 계산 버퍼 (GC 방지)
    private SkillStat _staticStat = new();              // 정적 스탯 베이스(baseStat + passiveBaseAdds) * commonStat
    private bool _statDirty = false;                 // 더티 플래그 / true일 때 재계산

    protected ActiveSkill _skill;                    // 액티브 스킬 (스탯 재계산용)
    protected Rigidbody2D _rigidbody;                // 속도용
    protected PoolObject _poolObject;                // 풀링용

    protected Vector2 _dir;                          // 방향
    protected float _createTime;                     // 생성 시간
    protected float _lastDurationTickTime;           // 최근 지속시간 틱 시간
    protected bool _expired = false;                 // 수명 만료 상태 (중복 방지, 추후 연출용)

    protected List<PassiveModifier> _passiveModifiers;    // 패시브 모디파이어

    protected SkillAnimator _animator;      // 연출용 애니메이터

    protected virtual void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _poolObject = GetComponent<PoolObject>();
        _animator = GetComponent<SkillAnimator>();
    }
    protected void Init(ActiveSkill owner, Vector2 dir)
    {
        // 리셋
        _expired = false;
        _statDirty = false;
        _lastDurationTickTime = 0f;

        _skill = owner;
        _dir = dir;

        // 스킬 스탯 스냅샷
        _runtimeBaseStat.CopyFrom(owner.BaseStat);
        _runtimeCommonStat.CopyFrom(owner.CommonStat);
        _runtimeUpgradeStat.CopyFrom(owner.UpgradeStat);
        _runtimePassiveMulStat.CopyFrom(owner.PassiveMulStat);
        _runtimeFinalStat.CopyFrom(owner.FinalStat);

        // 패시브 모디파이어 캐싱
        _passiveModifiers = new List<PassiveModifier>(owner.PassiveModifiers);

        _createTime = Time.time;
        _lastDurationTickTime = Time.time;

        // 정적 스탯 베이크
        BakeStaticStat();
        
        // 초기화 시 전용 모디파이어, 패시브 처리
        OnInit();

        // 리셋
        if (_animator != null)
            _animator.ResetState();

        // 크기 적용
        ApplySize();

        if (_animator != null)
        {
            // 초기화 후 자식에서 설정할 것
            OnBeforeStartSequence();
            // 발동 연출 시작
            _animator.StartSequence(_runtimeFinalStat.Duration);
        }
    }

    protected virtual void Update()
    {
        // 수명 체크
        if (_expired == false && Time.time >= _createTime + _runtimeFinalStat.Duration)
            ExpireObject();

        // 스킬 지속 시간 틱 체크 (스노우볼링)
        UpdateDurationTick();
        
        // 이동
        // 방향 벡터 제곱 길이가 0보다 클 때만
        if (_dir.sqrMagnitude > 0f)
        {
            transform.Translate(_dir * (_runtimeFinalStat.ProjectileSpeed * Time.deltaTime), Space.World);
        }
    }

    // 수명체크 따로 하는 애들 때문에 분리
    protected void UpdateDurationTick()
    {
        if (_runtimeFinalStat.DurationTickInterval <= 0f) return;
        if (Time.time >= _lastDurationTickTime + _runtimeFinalStat.DurationTickInterval)
        {
            OnDurationTick();
            _lastDurationTickTime = Time.time;
        }
    }

    // (baseStat + passiveBaseAdds) * commonStat 를 _staticBase에 저장
    // 지금 구조상 Init에서만 호출하면 되는데 패시브 구성이 바뀔 때 호출하면 됨
    private void BakeStaticStat()
    {
        if (_staticStat == null) _staticStat = new SkillStat();

        // 기본 스탯 복사
        _staticStat.CopyFrom(_runtimeBaseStat);

        // 패시브 깡 스탯 적용
        foreach (var passive in _passiveModifiers)
            passive.ModifyBaseAdd(_staticStat);

        // 공용 스탯 곱
        _staticStat.Multiply(_runtimeCommonStat);
    }

    // 스탯 재계산
    protected void CalculateStat()
    {
        // 더티 플래그 활성화 되면
        if (_statDirty == false) return;

        // 버퍼에 staticBase 복사 (baseStat + passiveBaseAdds) * commonStat
        _calcBuffer.CopyFrom(_staticStat);

        // 업그레이드 스탯 합 (관통 시 추가 피해, 추가추가피해 패시브 등)
        _calcBuffer.Add(_runtimeUpgradeStat);

        // 패시브 계수 합을 곱 (임플란트, 스노우볼링 등)
        _calcBuffer.Multiply(_runtimePassiveMulStat);

        // 패시브 최종 곱 (냥빨래, 황금왕관 등)
        foreach (var passive in _passiveModifiers)
            passive.ModifyFinal(_calcBuffer);

        // 결과를 _runtimeFinalStat에 덮어쓰기 (new X)
        _runtimeFinalStat.CopyFrom(_calcBuffer);

        // 물리 적용
        ApplySize();

        // 계산 했으니 끄기
        _statDirty = false;
    }

    // 크기 적용
    protected void ApplySize()
    {
        transform.localScale = Vector3.one * _runtimeFinalStat.Size;
    }

    // 수명 만료
    protected void ExpireObject()
    {
        if (_expired == true) return;
        _expired = true;

        // 수명 만료 시 실행될 로직
        OnExpire();

        if (_animator != null)
            _animator.RequestExpire(ReturnToPool);
        else
            ReturnToPool();
    }

    // 풀 반환
    protected void ReturnToPool()
    {
        if (_poolObject != null) _poolObject.ReturnToPool();
        else Destroy(gameObject);
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
        {
            BakeStaticStat();   // OnCustomInit에서 _runtimeBaseStat 바꼈을 수 있음 그래서 딱 한 번 다시 굽기
            SetDirty();
            CalculateStat();
        }
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
        SetDirty();
        CalculateStat();
    }

    // 스킬 애니메이터 발동 연출 시작 전
    // 자식의 설정 오버라이드
    protected virtual void OnBeforeStartSequence() { }


    // 수명 만료 시 호출
    // 파괴될 때 추가 로직, 연출 후 파괴되게
    protected virtual void OnExpire() { }

    // 스탯 재계산 더티 플래그
    protected void SetDirty() => _statDirty = true;
}
