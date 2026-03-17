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
    [SerializeField] private List<ItemEnhanceData> _enhanceDataList;

    [Header("장비 등급")]
    [SerializeField] private List<EquipGradeData> _gradeDataList;


    private Dictionary<int, EquipData> _equipDict;


    public EQUIP_PARTS EquipmentPart => _equipmentPart;
    public override string Name => _name;
    public string Description => _script;
    public string IconClickEffect => _iconClickEffect;
    public string IconClickSound => _iconClickSound;
    public EnhancementEventChannelSO OnEquipEnhancement => _onEquipEnhancement;

    private Dictionary<int, ItemEnhanceData> _enhances;

    private Dictionary<EQUIP_GRADE, EquipGradeData> _grades;


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
    public ItemEnhanceData GetEnhanceData(int id, int enhanceLevel)
    {
        if (_equipDict == null)
            Init();

        if(!_equipDict.TryGetValue(id, out var equip))
        { Debug.LogError($"{_name}는 강화 <color=yellow>{enhanceLevel}레벨</color>이 없습니다."); return null; }

        foreach(var enhance in equip.Enhances)
        {
            if(enhance.level == enhanceLevel)
                return new ItemEnhanceData(enhance);
        }

         return null;

        //if(!_enhances.TryGetValue(enhanceLevel, out var enhanceData))
        //    Debug.LogError($"{_name}는 강화 <color=yellow>{enhanceLevel}레벨</color>이 없습니다.");

    }

    // 해당 등급 데이터를 반환
    public EquipGradeData GetGradeData(int id, GRADE grade)
    {
        if (_equipDict == null)
            Init();

        if (!_equipDict.TryGetValue(id, out var equip))
        { Debug.LogError($"{_name}는 <color=yellow>{grade}</color>등급이 없습니다."); return null; }

        foreach (var gradeData in equip.Grades)
        {
            if (gradeData.grade == grade)
                return new EquipGradeData(gradeData);
        }

        return null;


        //if (!_grades.TryGetValue(grade, out var gradeData))
        //    Debug.LogError($"{_name}는 <color=yellow>{grade}</color>등급이 없습니다.");

    }


    private void Init()
    {
        var equipData = DataManager.Instance.GetSOData<EquipitemSO>();
        _equipDict = new Dictionary<int, EquipData>();
        foreach (var equip in equipData.dataList)
        {
            _equipDict.Add(equip.id, new EquipData());
            _equipDict[equip.id].GroupID = equip.group_id;
        }

        var equipEnhanceData = DataManager.Instance.GetSOData<EquipEnhanceSO>();
        foreach (var enhance in equipEnhanceData.dataList)
        {
            foreach(var equip in _equipDict.Values)
            {
                if(equip.GroupID == enhance.group_id)
                {
                    equip.Enhances.Add(enhance);
                    break;
                }
            }
        }

        var equipGradeData = DataManager.Instance.GetSOData<EquipGradeSO>();
        foreach (var grade in equipGradeData.dataList)
        {
            foreach (var equip in _equipDict.Values)
            {
                if (equip.GroupID == grade.group_id)
                {
                    equip.Grades.Add(grade);
                    break;
                }
            }
        }




        //_enhances = new Dictionary<int, ItemEnhanceData> { { 0, new ItemEnhanceData() } };
        //foreach(var enhanceData in _enhanceDataList)
        //{
        //    if (!_enhances.TryAdd(enhanceData.Level, enhanceData))
        //        Debug.LogError($"{name}의 강화레벨이 중복됐습니다.");
        //}

        //_grades = new Dictionary<EQUIP_GRADE, EquipGradeData>();
        //foreach(var gradeData in _gradeDataList)
        //{
        //    if (!_grades.TryAdd(gradeData.Grade, gradeData))
        //        Debug.LogError($"{name}의 등급이 중복됐습니다.");
        //}
    }

    public class EquipData
    {
        public int GroupID;
        public Equipitem Equipmnet;
        public List<EquipEnhance> Enhances;
        public List<EquipGrade> Grades;
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
    public class EquipGradeData
    {
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public string Desc { get; private set; }
        [field: SerializeField] public int GroupID { get; private set; }
        [field: SerializeField] public GRADE Grade { get; private set; }
        [field: SerializeField] public bool IsMaxGrade { get; private set; }
        [field: SerializeField] public int CostID { get; private set; }
        [field: SerializeField] public int CostCount { get; private set; }
        [field: SerializeField] public float StatRate { get; private set; }

        public EquipGradeData(EquipGrade grade)
        {
            ID = grade.id;
            Desc = grade.desc;
            GroupID = grade.group_id;
            Grade = grade.grade;
            IsMaxGrade = grade.is_max_grade;
            CostID = grade.cost_id;
            CostCount = grade.cost_count;
            StatRate = grade.stat_rate;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(_uniqueID != _iD)
            Debug.LogWarning($"{name}의 고유ID와 장비ID가 다릅니다.");
    }

#endif
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
    Grimy,      // 꼬질
    Fresh,      // 향긋
    Shiny,      // 반짝
    Pristine,   // 깔끔
    None        // 아무 등급 없음 
}