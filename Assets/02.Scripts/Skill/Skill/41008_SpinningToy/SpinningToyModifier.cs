using System.Collections.Generic;
using UnityEngine;
using static SpinningToySkill;

public class SpinningToyModifierData
{
    public bool BurstOnExpire = false;
    public WaitForSeconds BurstDelay;

    // 업그레이드로 추가된 타입별 투사체 목록
    public List<(TOY_TYPE type, int count)> AddedToys = new();
}


// 40042 기차 장난감 - 플레이어 속도 증가
public class SpinningToyTrainModifier : ActiveModifier<SpinningToySkill>
{
    [Header("증가 플레이어 속도")]
    [SerializeField] private float _playerSpeed;
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Train, upgradeData.ProjectileCount));
}

// 40043 팽이 장난감 - 넉백 추가
public class SpinningToyTopModifier : ActiveModifier<SpinningToySkill>
{
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Top, upgradeData.ProjectileCount));
}

// 40044 달 장난감 - 플레이어 체력 증가
public class SpinningToyMoonModifier : ActiveModifier<SpinningToySkill>
{
    [Header("증가 플레이어 체력")]
    [SerializeField] private float _playerHp;
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Moon, upgradeData.ProjectileCount));
}

// 40045 오리 장난감
public class SpinningToyDuckModifier : ActiveModifier<SpinningToySkill>
{
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Duck, upgradeData.ProjectileCount));
}

// 40046 해적선 장난감 - 적이 강해짐, 경험치 추가
public class SpinningToyPirateModifier : ActiveModifier<SpinningToySkill>
{
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Pirate, upgradeData.ProjectileCount));
}

// 40047 상어 장난감 - 플레이어 공격력 증가
public class SpinningToySharkModifier : ActiveModifier<SpinningToySkill>
{
    [Header("증가 플레이어 공격력")]
    [SerializeField] private float _playerDamage;
    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
        => skill._modifierData.AddedToys.Add((TOY_TYPE.Shark, upgradeData.ProjectileCount));
}

// 40048 물놀이 끝! - 지속시간 종료 시 퍼짐
public class SpinningToyBurstModifier : ActiveModifier<SpinningToySkill>
{
    [Header("버스트 사이 딜레이")]
    [SerializeField] private float _burstDelay = 0.4f;

    public override void ApplyModifier(SpinningToySkill skill, ActiveUpgradeData upgradeData)
    {
        if(skill._modifierData.BurstDelay == null)
            skill._modifierData.BurstDelay = new WaitForSeconds(_burstDelay);
        skill._modifierData.BurstOnExpire = true;
    }
}