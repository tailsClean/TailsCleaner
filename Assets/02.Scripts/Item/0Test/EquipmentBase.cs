using System.Collections.Generic;
using UnityEngine;
using static EquipmentGradeData;


public class EquipmentBase : PlayerEnhancement
{
    [Header("장비 정보")]
    public bool IsEquip;
    [field: SerializeField] public PARTS EquipmentPart { get; private set; }

    #region 장비 스텟
    // 임시 스텟
    [Header("장비 스텟")]
    [field: SerializeField] public int StatID { get; private set; }
    [field: SerializeField] public int StatGroupID { get; private set; }
    [field: SerializeField] public STAT StatType { get; private set; }
    [field: SerializeField] public int StatValue { get; private set; }

    public Dictionary<STAT , EquipmentIncreaseStat> IncreaseStat { get; private set; }

    public void SetIncreaseStat(STAT statType) => IncreaseStat.Add(statType, new EquipmentIncreaseStat(this));
    #endregion


    #region 장비 등급 데이터
    // 임시 등급
    [Header("장비 등급")]
    public bool IsGrade;
    [field: SerializeField] public int GradeID { get; private set; }
    [field: SerializeField] public int GradeGroupID { get; private set; }
    [field: SerializeField] public EQUIP_GRADE Grade { get; private set; }
    [field: SerializeField] public bool IsGradeMaxGrade { get; private set; }
    [field: SerializeField] public int GradeCostID { get; private set; }
    [field: SerializeField] public int GradeCostCount { get; private set; }
    [field: SerializeField] public float GradeStatRate { get; private set; } = 1;   // 스텟 증가량(곱연산)
    [field: SerializeField] public int GradePrice { get; private set; }             // 등급별 판매 가격

    public EquipmentGradeData EquipGrade { get; private set; }

    public void OnUpgrade()
    {
        Grade++;
        EquipGrade.OnUpgrade();
    }
    #endregion


    protected override void Awake()
    {
        base.Awake();
        IncreaseStat = new();
        SetIncreaseStat(StatType);
        EquipGrade = new EquipmentGradeData(this);
    }


    // 최종 스텟 증가량 제공 메서드(장비 증가량, 강화 증가량, 등급 증가량)
    public float GetIncreaseStat(STAT stat)
    {
        //
        float statValue = 0;
        if (IncreaseStat.TryGetValue(stat, out var equipmentStat))
            statValue = equipmentStat.StatUpValue;
        //
        
        float enhanceValue = EquipEnhance.AddValue;
        float gradeValue = EquipGrade.StatRate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return result;
    }

    public override Sprite GetSprite() => SpriteImage;                      // 수정 필요


    // 장비의 스텟증가량을 저장하는 클래스
    public class EquipmentIncreaseStat
    {
        public int ID { get; private set; }
        public int GroupID { get; private set; }
        public STAT StatUp { get; private set; }
        public int StatUpValue { get; private set; }

        public EquipmentIncreaseStat(EquipmentBase equip)
        {
            ID = equip.StatID;
            GroupID = equip.StatGroupID;
            StatUp = equip.StatType;
            StatUpValue = equip.StatValue;
        }
    }


    public enum PARTS
    {
        Weapon, Hat, Cloak, Shoes
    }

    public enum STAT
    {
        AttackPower, CriticalChance, MaxHp, DefensePower, MoveSpeed, EvasionChance
    }

}
