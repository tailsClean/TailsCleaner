using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentSO", menuName = "ItemData/Equipment")]
public class EquipmentSO : ItemBaseSO
{
    [Header("장비 정보")]
    [SerializeField] private int _iD;
    [SerializeField] private EQUIP_PARTS _equipmentPart;
    [SerializeField] private int _groupID;                      // 해당 파츠 고유ID
    [SerializeField] private string _name;
    [SerializeField] private string _script;
    [SerializeField] private string _iconSprite;
    [SerializeField] private string _iconClickEffect;
    [SerializeField] private string _iconClickSound;

    [Header("장비 스텟")]
    [SerializeField] private EnhancementEventChannelSO _onEquipEnhancement;
    [SerializeField] private List<IncreaseStat> _increaseStatList;

    [Header("장비 강화")]
    [SerializeField] private List<EnhanceData> _enhanceDataList;

    [Header("장비 등급")]
    [SerializeField] private List<GradeData> _gradeDataList;


    public EQUIP_PARTS EquipmentPart => _equipmentPart;
    public string Name => _name;
    public string Description => _script;
    public string IconClickEffect => _iconClickEffect;
    public string IconClickSound => _iconClickSound;

    private Dictionary<int, EnhanceData> _enhances;

    private Dictionary<EQUIP_GRADE, GradeData> _grades;


    // 특정 스탯 증가량을 찾아서 반환
    public int GetIncreaseStat(EQUIP_STAT stat)
    {
        foreach(var statData in _increaseStatList)
        {
            if (statData.Type == stat)
                return statData.Value;
        }
        return 0;
    }

    // 해당 강화레벨 데이터를 반환
    public EnhanceData GetEnhanceData(int enhanceLevel)
    {
        if (_enhances == null)
            Init();

        if(!_enhances.TryGetValue(enhanceLevel, out var enhanceData))
            Debug.LogError($"{_name}는 강화 <color=yellow>{enhanceLevel}레벨</color>은 없습니다.");

        return enhanceData;
    }

    // 해당 등급 데이터를 반환
    public GradeData GetGradeData(EQUIP_GRADE grade)
    {
        if (_grades == null)
            Init();

        if (!_grades.TryGetValue(grade, out var gradeData))
            Debug.LogError($"{_name}는 <color=yellow>{grade}</color>등급이 없습니다.");

        return gradeData;
    }


    private void Init()
    {
        _grades = new Dictionary<EQUIP_GRADE, GradeData>();
        foreach(var gradeData in _gradeDataList)
        {
            _grades.Add(gradeData.Grade, gradeData);
        }

        _enhances = new Dictionary<int, EnhanceData> { { 0, new EnhanceData() } };
        foreach(var enhanceData in _enhanceDataList)
        {
            _enhances.Add(enhanceData.Level, enhanceData);
        }
    }


    // 장비의 스텟증가량 데이터 클래스
    [Serializable]
    public class IncreaseStat
    {
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public int GroupID { get; private set; }
        [field: SerializeField] public EQUIP_STAT Type { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
    }

    // 등급 데이터 클래스
    [Serializable]
    public class GradeData
    {
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public int GroupID { get; private set; }
        [field: SerializeField] public EQUIP_GRADE Grade { get; private set; }
        [field: SerializeField] public bool IsMaxGrade { get; private set; }
        [field: SerializeField] public int CostID { get; private set; }
        [field: SerializeField] public int CostCount { get; private set; }
        [field: SerializeField] public float StatRate { get; private set; }
        [field: SerializeField] public int Price { get; private set; }
    }
}

public enum EQUIP_PARTS
{
    Weapon, Hat, Cloak, Shoes
}

public enum EQUIP_STAT
{
    AttackPower, CriticalChance, MaxHp, DefensePower, MoveSpeed, EvasionChance
}

public enum EQUIP_GRADE
{
    Grimy,     // 꼬질
    Fresh,     // 향긋
    Shiny,     // 반짝
    Pristine   // 깔끔
}
