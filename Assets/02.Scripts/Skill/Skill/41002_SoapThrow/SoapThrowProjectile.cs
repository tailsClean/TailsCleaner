using UnityEngine;

public class SoapThrowProjectile : SkillProjectile<SoapThrowModifierData>
{
    protected override bool OnCustomInit()
    {
        // 비누 덩어리 관통 제거
        if (_modifierData.RemovePierce)
        {
            // 관통 스택만큼 피해 계수 추가
            _runtimeUpgradeStat.Damage += _runtimeFinalStat.PierceCount * _modifierData.DamagePerRemovalPierce;

            // 관통 제거
            _runtimeBaseStat.PierceCount = 0;
            _runtimeUpgradeStat.PierceCount = 0;

            // 스탯 재계산
            return true;
        }

        // 관통 제거 안했으면 재계산 필요 X
        return false;
    }

    // 관통 시
    protected override bool OnPierce()
    {
        // 비누 덩어리
        // 관통없이 즉시 파괴
        if (_modifierData.RemovePierce == true)
        {
            ExpireObject();
            return true;
        }

        // 거품내기
        // 관통 시 추가 피해
        PierceDamage();

        // 거품 가속
        // 관통 시 속도 증가
        PierceSpeed();

        // 감나빗!
        // 관통 후 재추적
        if (_modifierData.Retracking == true)
            RetargetEnemy();

        return false;
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

    // 관통 후 새로운 적 재추적
    private void RetargetEnemy()
    {
        Debug.Log("[SoapProjectile] 재추적 로직 실행");
        // 공격 방향 기준 가장 가까운 적 탐색 후 _dir 갱신
    }
}
