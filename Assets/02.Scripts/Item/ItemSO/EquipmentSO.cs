using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/Equipment")]
public class EquipmentSO : PlayerEnhancementSO
{
    [Header("장비 정보")]
    [SerializeField] private EQUIP_PARTS _equipmentPart;


    [Header("장비 스텟")]
    [SerializeField] private int _statID;
    [SerializeField] private int _statGroupID;
    [SerializeField] private EQUIP_STAT _statType;
    [SerializeField] private int _statValue;


    [Header("장비 등급")]
    [SerializeField] private int _gradeID;
    [SerializeField] private int _gradeGroupID;
    [SerializeField] private EQUIP_GRADE _grade;
    [SerializeField] private bool _isGradeMaxGrade;
    [SerializeField] private int _gradeCostID;
    [SerializeField] private int _gradeCostCount;
    [SerializeField] private float _gradeStatRate = 1;      // 스텟 증가량(곱연산)
    [SerializeField] private int _gradePrice;               // 등급별 판매 가격



    public EQUIP_PARTS EquipmentPart => _equipmentPart;
    public int StatID => _statID;
    public int StatGroupID => _statGroupID;
    public EQUIP_STAT StatType => _statType;
    public int StatValue => _statValue;
    public int GradeID => _gradeID;
    public int GradeGroupID => _gradeGroupID;
    public EQUIP_GRADE Grade => _grade;
    public bool IsGradeMaxGrade => _isGradeMaxGrade;
    public int GradeCostID => _gradeCostID;
    public int GradeCostCount => _gradeCostCount;
    public float GradeStatRate => _gradeStatRate;
    public int GradePrice => _gradePrice;



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
