using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 스텟 계산기
/// </summary>
public class PlayerStatCalculator
{
    private PlayerLoadout _playerLoadout;
    private PlayerStatFlat _plusStat;
    private PlayerStatMul _multipleStat;

    public PlayerStatCalculator(PlayerLoadout enhanceInventory)
    {
        _playerLoadout = enhanceInventory;

        if (_plusStat == null)
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
        float baseStat = initialStat;
        float plusStat = _plusStat.Get(stat);
        float multipleStat = _multipleStat.Get(stat);
        float divisionStat = _playerLoadout.GetRelicDivisionValue(stat);


        return (baseStat + plusStat + divisionStat) * (1 + multipleStat);
    }
}

# region 스킬 부여 스탯 확장 메서드

public static class PlayerStatFlatExtension
{
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
