using System.Collections.Generic;
using UnityEngine;


public class RelicDivisionSystem
{
    private PlayerLoadout _loadout;
    private List<RelicBase> _relics;

    public bool IsDivisionActive => DivitionActivable();
    public RelicDivision CurrentDivision { get; private set; }


    public RelicDivisionSystem(PlayerLoadout loadout)
    {
        _loadout = loadout;
        _relics = _loadout.MyRelics;
    }

    // 유물의 공명효과 증가 확인
    public float GetIncreaseStat(out PLAYER_STAT increaseStat)
    {
        increaseStat = PLAYER_STAT.None;
        if (!IsDivisionActive)
            return 0;

        increaseStat = GetIncreaseStatType();
        return CurrentDivision.value;
    }

    private bool DivitionActivable()
    {
        if(_loadout == null || _relics == null)
            return false;

        // 유물이 3개가 안 되면 활성화x
        if(_relics.Count < 3)
            return false;

        CurrentDivision = _relics[0].Data.Division;
        foreach(var relic in _relics)
        {
            var divition = relic.Data.Division;
            if (divition.division_type != CurrentDivision.division_type)
                return false;
        }

        return true;
    }

    private PLAYER_STAT GetIncreaseStatType()
    {
        var type = _relics![0].Data.Relic.stat_type;
        return type switch
        {
            STAT_TYPE.Gold => PLAYER_STAT.GoldGainRate,
            STAT_TYPE.Equipment => PLAYER_STAT.EquipmentDropRate,
            STAT_TYPE.Exp => PLAYER_STAT.ExpGainRate,
            STAT_TYPE.ItemRange => PLAYER_STAT.ItemDropRate,
            _ => PLAYER_STAT.None
        };
    }

}
