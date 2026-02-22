using System.Collections.Generic;
using UnityEngine;
using static PassiveSkillData;

public class SoapThrowProjectile : MonoBehaviour
{
    private SkillStat _runtimeBaseStat;         // 런타임 기본 스탯
    private SkillStat _runtimeCommonStat;       // 런타임 공용 스탯
    private SkillStat _runtimeUpgradeStat;      // 런타임 업그레이드 스탯
    private SkillStat _runtimePassiveMulStat;   // 런타임 패시브 배율 합 , 임플란트 누적
    private SkillStat _runtimeFinalStat;        // 최종 스탯

    private ActiveSkill _skill;                 // 액티브 스킬 (스탯 재계산용)
    private Rigidbody2D _rigidbody;             // 리지드바디
    private Collider2D _collider;               // 콜라이더

    private Vector2 _dir;                       // 방향
    private float _createTime;                  // 생성 시간
    private int _currentPierceCount = 0;        // 현재 관통 횟수

    private SoapThrowModifierData _modifierData;        // 전용 모디파이어  (나중에 제네릭으로 T 타입으로 바꾸면될듯)
    private List<PassiveModifier> _passiveModifiers;    // 패시브 모디파이어

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponentInChildren<Collider2D>();
    }

    public void Init(ActiveSkill owner, SoapThrowModifierData modifierData, Vector2 dir)
    {
        // 스킬 참조
        _skill = owner;
        // 스킬 스탯 복사
        _runtimeBaseStat = owner.BaseStat.Clone();
        _runtimeCommonStat = owner.CommonStat.Clone();
        _runtimeUpgradeStat = owner.UpgradeStat.Clone();
        _runtimePassiveMulStat = owner.PassiveMulStat.Clone();
        _runtimeFinalStat = owner.FinalStat.Clone();
        // 전용 모디파이어 장착
        _modifierData = modifierData;
        // 패시브 모디파이어 캐싱
        _passiveModifiers = new List<PassiveModifier>(owner.PassiveModifiers);
        // 방향
        _dir = dir;
        // 생성 시간
        _createTime = Time.time;

        // 재계산 여부
        bool recalcul = false;

        // 비누 덩어리 (관통 제거)
        if (_modifierData.RemovePierce)
        {
            // 업그레이드 피해 계수에 사라진 관통 스택만큼 피해 계수 추가
            _runtimeUpgradeStat.Damage += _runtimeFinalStat.PierceCount * _modifierData.DamagePerRemovalPierce;
            _runtimeBaseStat.PierceCount = 0;
            _runtimeUpgradeStat.PierceCount = 0;
            recalcul = true;
        }

        // 투사체 생성 시 패시브 조건부 스탯 적용
        foreach (var passive in _passiveModifiers)
        {
            if (passive.OnProjectileInit(_runtimeBaseStat, _runtimeFinalStat))
            {
                recalcul = true;
            }
        }

        // 스탯 재계산
        if (recalcul == true) CalculateStat();

        // 속도, 크기 적용
        ApplyPhysics();
    }

    private void Update()
    {
        // 수명 시간 체크
        if (Time.time >= _createTime + _runtimeBaseStat.Duration)
        {
            DestroyProjectile();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 적 충돌
        if (collision.CompareTag("Monster"))
        {
            // 적 피해 주기
            if(collision.TryGetComponent<MonsterBase>(out MonsterBase monster))
            {
                monster.TakeDamage(_runtimeFinalStat.Damage);
            }

            // 비누 덩어리 (관통 제거)
            if (_modifierData.RemovePierce == true)
            {
                DestroyProjectile();
                return;
            }

            // 관통 증가
            _currentPierceCount++;

            // 거품내기 관통 시 추가 피해
            PierceDamage();

            // 거품 가속 : 관통 시 속도 증가
            PierceSpeed();

            // 패시브 관통 효과 (임플란트)
            foreach (var passive in _passiveModifiers)
                passive.OnPierce(_runtimePassiveMulStat);

            // 재계산
            CalculateStat();

            // 물리 적용
            ApplyPhysics();

            // 관통 횟수 초과 시 파괴
            if (_currentPierceCount > _runtimeFinalStat.PierceCount)
            {
                DestroyProjectile();
                return;
            }

            // 감나빗! (관통 후 재추적)
            if (_modifierData.Retracking)
                RetargetEnemy();
        }
    }

    // 관통 추가 피해
    private void PierceDamage()
    {
        // 패시브 없거나 혹시나 관통당 추가 피해 없으면 무시
        if (_modifierData.PierceDamage == false || _modifierData.DamagePerPierce <= 0f) return;

        // 전용 모디파이어 관통 추가 피해
        float bonus = _modifierData.DamagePerPierce;

        // 추가 추가 피해 있을 때 (0.5 * 2)
        if (_runtimeFinalStat.ExtraDamageMultiplier > 1)
            bonus *= _runtimeFinalStat.ExtraDamageMultiplier;

        // 업그레이드 피해에 추가
        _runtimeUpgradeStat.Damage += bonus;
    }

    // 관통 추가 속도
    private void PierceSpeed()
    {
        // 패시브 없거나 혹시나 관통당 추가 속도 없으면 무시
        if (_modifierData.PierceSpeed == false || _modifierData.SpeedPerPierce <= 0f) return;

        // 업그레이드 스탯에 추가
        _runtimeUpgradeStat.ProjectileSpeed += _modifierData.SpeedPerPierce;
    }

    // 스탯 계산
    private void CalculateStat()
    {
        _runtimeFinalStat = _skill.GetFinalStat(_runtimeBaseStat, _runtimeCommonStat, _runtimeUpgradeStat, _runtimePassiveMulStat);
    }

    // 속도, 크기 적용
    private void ApplyPhysics()
    {
        // 속도 설정
        if (_rigidbody != null) _rigidbody.linearVelocity = _dir * _runtimeBaseStat.ProjectileSpeed;

        // 크기 설정
        transform.localScale = Vector3.one * _runtimeBaseStat.Size;
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
