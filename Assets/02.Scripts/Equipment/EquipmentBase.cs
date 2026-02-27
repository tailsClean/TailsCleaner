using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EquipmentGrade;

public class EquipmentBase : PlayerEnhancement
{
    [Header("장비 정보")]
    [field: SerializeField] public PARTS EquipmentPart { get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; }        // 최대 소지 수량

    #region 장비 스텟
    // 임시 스텟
    [Header("장비 스텟")]
    [field: SerializeField] public int StatID { get; private set; }
    [field: SerializeField] public int StatGroupID { get; private set; }
    [field: SerializeField] public STAT StatType { get; private set; }
    [field: SerializeField] public int StatValue { get; private set; }

    public Dictionary<STAT , EquipmentIncreaseStat> IncreaseStat { get; private set; }

    public void AddIncreaseStat(STAT statType) => IncreaseStat.Add(statType, new EquipmentIncreaseStat(this));
    #endregion


    #region 장비 등급 데이터
    // 임시 등급
    [field: SerializeField] public int GradeID { get; private set; }
    [field: SerializeField] public int GradeGroupID { get; private set; }
    [field: SerializeField] public GRADE Grade { get; private set; }
    [field: SerializeField] public bool IsGradeMaxGrade { get; private set; }
    [field: SerializeField] public int GradeCostID { get; private set; }
    [field: SerializeField] public int GradeCostCount { get; private set; }
    [field: SerializeField] public float GradeStatRate { get; private set; } = 1;   // 스텟 증가량(곱연산)
    [field: SerializeField] public int GradePrice { get; private set; }             // 등급별 판매 가격

    public EquipmentGrade EquipGrade { get; private set; }
    #endregion

    public void OnUpgrade()
    {
        Grade++;
        EquipGrade.OnUpgrade();
    }


    protected override void Awake()
    {
        base.Awake();
        IncreaseStat = new();
        AddIncreaseStat(StatType);
        EquipGrade = new EquipmentGrade(this);
    }


    // 최종 스텟 증가량 제공 메서드(장비 증가량, 강화 증가량, 등급 증가량)
    public int GetIncreaseStat(STAT stat)
    {
        float statValue = IncreaseStat[stat].Value;
        float enhanceValue = EquipEnhance.AddValue;
        float gradeValue = EquipGrade.StatRate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return (int)result;
    }


    // 장비의 스텟증가량을 저장하는 클래스
    public class EquipmentIncreaseStat
    {
        public int ID { get; private set; }
        public int GroupID { get; private set; }
        public STAT Type { get; private set; }
        public int Value { get; private set; }

        public EquipmentIncreaseStat(EquipmentBase equip)
        {
            ID = equip.StatID;
            GroupID = equip.StatGroupID;
            Type = equip.StatType;
            Value = equip.StatValue;
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
