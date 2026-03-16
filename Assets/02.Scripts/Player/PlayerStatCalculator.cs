using System.Collections.Generic;
using UnityEngine;
using static EquipmentSO;

/// <summary>
/// 플레이어 스텟 계산기
/// </summary>
public class PlayerStatCalculator
{
    private PlayerLoadout _playerLoadout;
    private ILevelStat _levelStat;
    private PlayerStatFlat _plusStat;
    private PlayerStatMul _multipleStat;

    public PlayerStatCalculator(PlayerLoadout enhanceInventory, ILevelStat levelStat)
    {
        _playerLoadout = enhanceInventory;
        _levelStat = levelStat;

        if(_plusStat == null)
            _plusStat = new PlayerStatFlat();
        if(_multipleStat == null)
            _multipleStat = new PlayerStatMul();
    }

    // 스킬 스탯값 세팅
    public void SetSkillStat(PlayerStatFlat flat, PlayerStatMul multi)
    {
        _plusStat = flat;
        _multipleStat = multi;
    }

    // 최종 스탯 계산값
    public float GetFinalSat(float initialStat, PLAYER_STAT stat)
    {
        float baseStat = initialStat + _levelStat.Get(stat);
        float itemStat = GetItemStat(stat);
        float plusStat = _plusStat.Get(stat);
        float multipleStat = _multipleStat.Get(stat);

        Debug.Log("레벨: " + stat + " / " + baseStat);
        Debug.Log("아이템: " + stat + " / " + itemStat);

        return baseStat + itemStat + plusStat + multipleStat;
    }

    
    private float Calculate(float baseStat, float itemStat, float plusStat, float multipleStat, PLAYER_STAT stat)
    {
        return 0;
    }

    private float GetItemStat(PLAYER_STAT stat)
    {
        return stat switch
        {
            // 장비로 상승하는 값
            PLAYER_STAT.MaxHp => _playerLoadout.GetIncreaseStat(EQUIP_STAT.MaxHp),
            PLAYER_STAT.AttackPower => _playerLoadout.GetIncreaseStat(EQUIP_STAT.AttackPower),
            PLAYER_STAT.DefensePower => _playerLoadout.GetIncreaseStat(EQUIP_STAT.DefensePower),
            PLAYER_STAT.CriticalChance => _playerLoadout.GetIncreaseStat(EQUIP_STAT.CriticalChance),
            PLAYER_STAT.EvasionChance => _playerLoadout.GetIncreaseStat(EQUIP_STAT.EvasionChance),
            PLAYER_STAT.MoveSpeed => _playerLoadout.GetIncreaseStat(EQUIP_STAT.MoveSpeed),
            
            // 유물로 상승하는 값
            PLAYER_STAT.GoldGainRate => _playerLoadout.GetIncreaseStat(RELIC_STAT.GoldGainRate),
            PLAYER_STAT.ItemDropRate => _playerLoadout.GetIncreaseStat(RELIC_STAT.ItemDropRate),
            PLAYER_STAT.ExpGainRate => _playerLoadout.GetIncreaseStat(RELIC_STAT.ExpGainRate),
            _ => 0f
        };
    }

}

# region 확장 메서드

public static class PlayerStatFlatExtension
{
    // 레벨에 따른 스탯증가량
    public static float Get(this ILevelStat stat, PLAYER_STAT type)
    {
        return type switch
        {
            PLAYER_STAT.MaxHp or
            PLAYER_STAT.AttackPower or
            PLAYER_STAT.DefensePower or
            PLAYER_STAT.HealthRegen => stat.StatGrowth,
            _ => 0f
        };
    }

    // 스킬에서 더해지는 스탯값
    public static float Get(this PlayerStatFlat stat, PLAYER_STAT type)
    {
        return type switch
        {
            PLAYER_STAT.MaxHp => stat.MaxHp,
            PLAYER_STAT.AttackPower => stat.AttackPower,
            PLAYER_STAT.DefensePower => stat.DefensePower,
            PLAYER_STAT.CriticalChance => stat.CriticalChance,
            PLAYER_STAT.CriticalDamageMultiplier => stat.CriticalDamageMultiplier,
            PLAYER_STAT.EvasionChance => stat.DefensePower,
            PLAYER_STAT.MoveSpeed => stat.MoveSpeed,
            PLAYER_STAT.PickupRange => stat.PickupRange,
            PLAYER_STAT.ExpGainRate => stat.ExpGainRate,
            _ => 0f
        };
    }

    // 스킬에서 곱해지는 스탯값
    public static float Get(this PlayerStatMul stat, PLAYER_STAT type)
    {
        return type switch
        {
            PLAYER_STAT.MaxHp => stat.MaxHp,
            PLAYER_STAT.AttackPower => stat.AttackPower,
            PLAYER_STAT.DefensePower => stat.DefensePower,
            PLAYER_STAT.CriticalChance => stat.CriticalChance,
            PLAYER_STAT.CriticalDamageMultiplier => stat.CriticalDamageMultiplier,
            PLAYER_STAT.EvasionChance => stat.DefensePower,
            PLAYER_STAT.MoveSpeed => stat.MoveSpeed,
            PLAYER_STAT.PickupRange => stat.PickupRange,
            PLAYER_STAT.ExpGainRate => stat.ExpGainRate,
            _ => 1f
        };
    }
}

#endregion
