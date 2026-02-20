using System.Collections.Generic;
using UnityEngine;
using static PassiveSkillData;

public class SoapThrowProjectile : MonoBehaviour
{
    private SkillStat _runtimeBaseStat;         // 런타임 기본 스탯
    private SkillStat _runtimeCommonStat;       // 런타임 공용 스탯
    private SkillStat _runtimeUpgradeStat;      // 런타임 업그레이드 스탯
    private SkillStat _runtimeFinalStat;        // 최종 스탯

    private ActiveSkill _skill;                 // 액티브 스킬 (패시브 접근용)
    private Rigidbody2D _rigidbody;             // 리지드바디
    private Collider2D _collider;               // 콜라이더

    private Vector2 _dir;                       // 방향
    private float _createTime;                  // 생성 시간
    private int _currentPierceCount = 0;        // 현재 관통 횟수

    private SoapThrowModifierData _modifierData;    // 전용 모디파이어
    private HashSet<int> _activePassiveIds;         // 활성화된 패시브 로직 ID

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponentInChildren<Collider2D>();
    }

    public void Init(ActiveSkill owner, SoapThrowModifierData modifierData, Vector2 dir)
    {
        // 스킬 스탯 복사
        _runtimeBaseStat = owner.BaseStat.Clone();
        _runtimeCommonStat = owner.CommonStat.Clone();
        _runtimeUpgradeStat = owner.UpgradeStat.Clone();
        _runtimeFinalStat = owner.FinalStat.Clone();
        // 전용 모디파이어 장착
        _modifierData = modifierData;
        // 패시브 로직 설정
        _activePassiveIds = new HashSet<int>(owner.ActivePassiveIds);
        // 방향
        _dir = dir;
        // 생성 시간
        _createTime = Time.time;

        // 속도, 크기 적용
        ApplyPhysics();

        // 재계산 여부
        bool recalcul = false;

        // 나중에 수치들 SO로 빼서 인스펙터에서 관리가능하게 만들어야함. (기획팀 수정 용이)

        // 비누 덩어리 (관통 제거)
        if (_modifierData.RemovePierce == true)
        {
            _runtimeBaseStat.Damage = _runtimeFinalStat.PierceCount * 2f;
            _runtimeFinalStat.PierceCount = 0;
            recalcul = true;
        }


        if (recalcul == true)
            CalculateStat();
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

            // 재계산 여부
            bool recalcul = false;

            // 관통 시 추가 피해, 속도
            recalcul = PierceDamage() || PierceSpeed();

            // 만약 목표를 중앙에 두고 스위치 패시브가
            // 런타임 도중에도 추가 넉백 수치가 적용 된다면
            // 추가해야함

            // 재계산
            if (recalcul == true)
                CalculateStat();

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
    private bool PierceDamage()
    {
        // 관통 시 추가 피해
        if (_modifierData.PierceDamage == true && _modifierData.DamagePerPierce > 0f)
        {
            // 전용 모디파이어
            float bonus = _modifierData.DamagePerPierce;

            // 패시브 ID
            int implantId = (int)PASSIVE_ID.Implant;
            int DoubleExtraDmg = (int)PASSIVE_ID.DoubleExtraDmg;

            // 임플란트 패시브
            //if (_activePassiveIds.Contains(implantId) == true)
            //    bonus += SkillDataLoader.GetPassiveSkillData(implantId).Config as ImplantConfig;
        
            // 추가추가피해 패시브
            if (_activePassiveIds.Contains(DoubleExtraDmg) == true)
                bonus *= 2f;
        
            // baseStat에 더하고 재계산
            _runtimeBaseStat.Damage += bonus;
        
            // 재계산
            return true;
        }
        

        return false;
    }

    // 관통 추가 속도
    private bool PierceSpeed()
    {
        // 관통 시 추가 속도
        //if (_modifierData.PierceSpeed == true && _modifierData.SpeedPerPierce > 0f)
        //{
        //    // 전용 모디파이어
        //    float bonus = _modifierData.SpeedPerPierce;
        //
        //    // baseStat에 더하고 재계산
        //    _runtimeBaseStat.ProjectileSpeed += bonus;
        //
        //    // 재계산
        //    return true;
        //}
        //
        return false;
    }





    // 스탯 계산
    private void CalculateStat()
    {
        _runtimeFinalStat = _skill.GetFinalStat(_runtimeBaseStat, _runtimeCommonStat, _runtimeUpgradeStat);
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
