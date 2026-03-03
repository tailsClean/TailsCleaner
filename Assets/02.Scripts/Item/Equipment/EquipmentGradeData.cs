using UnityEngine;


public class EquipmentGradeData
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public GRADE Grade { get; private set; }
    public bool IsMaxGrade { get; private set; }
    public int CostID { get; private set; }
    public int CostCount { get; private set; }
    public float StatRate { get; private set; }
    public int Price { get; private set; }

    public EquipmentGradeData(EquipmentBase equip)
    {
        ID = equip.GradeID;
        GroupID = equip.GradeGroupID;
        Grade = equip.Grade;
        IsMaxGrade = equip.IsGradeMaxGrade;
        CostID = equip.GradeCostID;
        CostCount = equip.GradeCostCount;
        StatRate = equip.GradeStatRate;
        Price = equip.GradePrice;
    }

    public void OnUpgrade()
    {
        Grade++;
    }

    public enum GRADE
    {
        Grimy,     // 꼬질
        Fresh,     // 향긋
        Shiny,     // 반짝
        Pristine   // 깔끔
    }
}
