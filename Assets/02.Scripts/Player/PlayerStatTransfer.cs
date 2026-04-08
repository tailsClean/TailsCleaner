

using System.Collections.Generic;

public class PlayerStatTransfer : IPlayerData
{

    private Dictionary<PLAYER_STAT, float> _statDict;


    private int _id;

    private CharManageTableSO _dataTable;           // 데이터매니저에서 데이터 캐싱

    public Dictionary<PLAYER_STAT, float> StatDict => _statDict;

    public PlayerStatTransfer()
    {
        _statDict = new Dictionary<PLAYER_STAT, float>();
    }


    #region 외부 참조 데이터

    public int ID => _id;

    public float Maxhp => _statDict[PLAYER_STAT.MaxHp];
    public float AttackPower => _statDict[PLAYER_STAT.AttackPower];
    public float DefensePower => _statDict[PLAYER_STAT.DefensePower];
    public float EvasionChance => _statDict[PLAYER_STAT.EvasionChance];
    public float CriticalChance => _statDict[PLAYER_STAT.CriticalChance];
    public float CriticalDamageMultiplier => _statDict[PLAYER_STAT.CriticalDamageMultiplier];
    public float CriticalResistance => _statDict[PLAYER_STAT.CriticalResistance];
    public float PickupRange => _statDict[PLAYER_STAT.PickupRange];
    public float MoveSpeed => _statDict[PLAYER_STAT.MoveSpeed];
    public float AttackSpeed => _statDict[PLAYER_STAT.MoveSpeed];
    public float ItemDropRate => _statDict[PLAYER_STAT.ItemDropRate];
    public float GoldGainRate => _statDict[PLAYER_STAT.GoldGainRate];
    public float HealthRegain => _statDict[PLAYER_STAT.HealthRegen];
    public float ExpGainRate => _statDict[PLAYER_STAT.ExpGainRate];


    #endregion


    // 플레이어의 가본 스탯을 불러오기
    public void SetBaseStat(int playerLevel)
    {
        if (_dataTable == null)
            _dataTable = DataManager.Instance.GetSOData<CharManageTableSO>();

        var levelData = _dataTable.GetById(playerLevel);
        _id = levelData.char_id;
        SetStatDict(PLAYER_STAT.MaxHp, levelData.char_hp_max);
        SetStatDict(PLAYER_STAT.AttackPower, levelData.char_atk);
        SetStatDict(PLAYER_STAT.DefensePower, levelData.char_def);
        SetStatDict(PLAYER_STAT.EvasionChance, levelData.char_evasion);
        SetStatDict(PLAYER_STAT.CriticalChance, levelData.char_crtical);
        SetStatDict(PLAYER_STAT.CriticalDamageMultiplier, levelData.char_crtical_damage);
        SetStatDict(PLAYER_STAT.CriticalResistance, levelData.char_anti_crtical);
        SetStatDict(PLAYER_STAT.HealthRegen, levelData.char_hp_regain);
        SetStatDict(PLAYER_STAT.PickupRange, levelData.char_item_range);
        SetStatDict(PLAYER_STAT.MoveSpeed, levelData.char_move_speed);
        SetStatDict(PLAYER_STAT.AttackSpeed, levelData.char_attack_speed);
        SetStatDict(PLAYER_STAT.ItemDropRate, levelData.char_dropbonus_equip);
        SetStatDict(PLAYER_STAT.GoldGainRate, levelData.char_dropbonus_gold);
        SetStatDict(PLAYER_STAT.ExpGainRate, levelData.char_dropbonus_exp);

    }

    private void SetStatDict(PLAYER_STAT stat, float value)
    {
        if(!_statDict.TryGetValue(stat, out float statValue))
            _statDict.Add(stat, value);

        else
            _statDict[stat] = value;
    }


    // 착용 중인 장비, 유물의 스탯 반영
    public void SetLoadoutStat(PlayerLoadout loadout)
    {
        // 장비로 상승하는 값
        _statDict[PLAYER_STAT.MaxHp] += loadout.GetIncreaseStat(EQUIP_STAT_TYPE.MaxHP);
        _statDict[PLAYER_STAT.AttackPower] *= loadout.GetIncreaseStat(EQUIP_STAT_TYPE.Attack);
        _statDict[PLAYER_STAT.DefensePower] *= loadout.GetIncreaseStat(EQUIP_STAT_TYPE.Defense);
        _statDict[PLAYER_STAT.EvasionChance] += loadout.GetIncreaseStat(EQUIP_STAT_TYPE.Dodge);
        _statDict[PLAYER_STAT.CriticalChance] += loadout.GetIncreaseStat(EQUIP_STAT_TYPE.CriticalRate);
        _statDict[PLAYER_STAT.MoveSpeed] += loadout.GetIncreaseStat(EQUIP_STAT_TYPE.MoveSpeed);

        // 유물로 상승하는 값
        _statDict[PLAYER_STAT.ItemDropRate] += loadout.GetIncreaseStat(STAT_TYPE.ItemRange);
        _statDict[PLAYER_STAT.GoldGainRate] += loadout.GetIncreaseStat(STAT_TYPE.Gold);
        _statDict[PLAYER_STAT.ExpGainRate] += loadout.GetIncreaseStat(STAT_TYPE.Exp);

        RelicDivisionStat(loadout);
    }

    // 유물의 공명 효과 반영
    private void RelicDivisionStat(PlayerLoadout loadout)
    {
        _statDict[PLAYER_STAT.ItemDropRate]  += loadout.GetRelicDivisionValue(PLAYER_STAT.ItemDropRate);
        _statDict[PLAYER_STAT.GoldGainRate]  += loadout.GetRelicDivisionValue(PLAYER_STAT.GoldGainRate);
        _statDict[PLAYER_STAT.ExpGainRate] += loadout.GetRelicDivisionValue(PLAYER_STAT.ExpGainRate);
    }

}



