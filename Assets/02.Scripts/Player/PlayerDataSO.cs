using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    public const int PlayerID = 1;

    [Header("플레이어 기본 스탯")]
    [SerializeField] private int _id;
    [SerializeField] private float _maxhp = 15;
    [SerializeField] private float _attackPower = 10;
    [SerializeField] private float _defensePower = 1;
    [SerializeField] private float _evasionChance = 10;                  // 회피율
    [SerializeField] private float _criticalChance = 10;
    [SerializeField] private float _criticalDamageMultiplier = 2;        // 치명타 피해 배율
    [SerializeField] private float _criticalResistance = 10;             // 치명 저항
    [SerializeField] private float _healthRegain = 10;                    // Hp 회복량
    [SerializeField] private float _pickupRange = 1;                     // 아이템 줍는 범위
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _attackSpeed = 5;
    [SerializeField] private float _outGameMaxExp = 50;

    [Header("획득량 증가")]
    [SerializeField] private float _itemDropRate = 1;
    [SerializeField] private float _goldGainRate = 1;
    [SerializeField] private float _expGainRate = 10;

    [Header("아웃게임 레벨업 데이터")]
    [SerializeField] private List<OutGameLevelData> _outGameLevelData;

    [Header("인게임 레벨업 데이터")]
    [SerializeField] private List<InGameLevelData> _inGameLevelData;

    #region 외부 참조 데이터

    public int ID  => _id;

    // 공격 관련 데이터
    public float AttackPower => _attackPower;
    public float CriticalChance => _criticalChance;
    public float CriticalDamageMultiplier => _criticalDamageMultiplier;
    public float AttackSpeed => _attackSpeed;

    // 방어 관련 데이터
    public float Maxhp => _maxhp;
    public float DefensePower => _defensePower;
    public float EvasionChance => _evasionChance;
    public float CriticalResistance => _criticalResistance;

    // 유틸리티 데이터
    public float HealthRegen => _healthRegain;
    public float PickupRange => _pickupRange;
    public float MoveSpeed => _moveSpeed;
    public float ItemDropRate => _itemDropRate;
    public float GoldGainRate => _goldGainRate;

    // 레벨업 데이터
    public float OutGameMaxExp => _outGameMaxExp;
    public float OutMaxLevel => _outGameLevelData.Count;
    public float InMaxLevel => _inGameLevelData.Count;
    public float ExpGainRate => _expGainRate;
    public OutGameLevelData GetOutLevelData(int level) =>_outGameLevelData[level - 1];
    public InGameLevelData GetInLevelData(int level) => _inGameLevelData[level - 1];

    #endregion


    /// <summary>
    ///  데이터테이블에서 값을 가져와 저장하는 초기화 코드
    /// </summary>
    /// <param name="id"></param>
    public void Init(int id)
    {
        var dataTable = DataManager.Instance.GetSOData<CharManageTableSO>();

        if (dataTable == null)
        { Debug.LogError("CharManageTableSO 데이터가 없습니다."); return; }

        _id = id;
        _maxhp = dataTable.GetById(id).char_hp_max;
        _attackPower = dataTable.GetById(id).char_atk;
        _defensePower = dataTable.GetById(id).char_def;
        _evasionChance = dataTable.GetById(id).char_evasion;
        _criticalChance = dataTable.GetById(id).char_crtical;
        _criticalDamageMultiplier = dataTable.GetById(id).char_crtical_damage;
        _criticalResistance = dataTable.GetById(id).char_anti_crtical;
        _healthRegain = dataTable.GetById(id).char_hp_regain;
        _pickupRange = dataTable.GetById(id).char_item_range;
        _moveSpeed = dataTable.GetById(id).char_move_speed;
        _attackSpeed = dataTable.GetById(id).char_attack_speed;
        _outGameMaxExp = dataTable.GetById(id).char_exp_limit;
        _itemDropRate = dataTable.GetById(id).char_dropbonus_equip;
        _goldGainRate = dataTable.GetById(id).char_dropbonus_gold;
        _expGainRate = dataTable.GetById(id).char_dropbonus_exp;

        InitLevelList();
    }

    private void InitLevelList()
    {
        var outLevelDataTable = DataManager.Instance.GetSOData<CharLevelTableSO>();
        _outGameLevelData = new List<OutGameLevelData>();
        foreach (var data in outLevelDataTable.dataList)
        {
            _outGameLevelData.Add(new OutGameLevelData(data.char_level, data.char_stat_growth, data.char_exp_limit));
        }

        var inLevelDataTable = DataManager.Instance.GetSOData<CharIngameTableSO>();
        _inGameLevelData = new List<InGameLevelData>();
        foreach (var data in inLevelDataTable.dataList)
        {
            _inGameLevelData.Add(new InGameLevelData(data.char_ingame_level, data.char_ingame_exp));
        }
    }


}

# region 레벨 시스템
[Serializable]
public class OutGameLevelData
{
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public float StatGrowth { get; private set; }
    [field: SerializeField] public float MaxExp { get; private set; }           // 배율 계산

    public OutGameLevelData(int level, float statGrowth, float maxExp)
    {
        Level = level;
        StatGrowth = statGrowth;
        MaxExp = maxExp;
    }
}

[Serializable]
public class InGameLevelData
{
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public float MaxExp { get; private set; }           // 해당 경험치통 자체

    public InGameLevelData(int level, float maxExp)
    {
        Level = level;
        MaxExp = maxExp;
    }
}
# endregion

public enum PLAYER_STAT
{
    MaxHp,
    AttackPower,
    DefensePower,
    CriticalChance,
    CriticalDamageMultiplier,
    EvasionChance,
    MoveSpeed,
    HealthRegen,
    PickupRange,
    GoldGainRate,
    ItemDropRate,
    ExpGainRate,
}