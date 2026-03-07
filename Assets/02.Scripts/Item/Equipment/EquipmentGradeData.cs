using UnityEngine;


public class EquipmentGradeData
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public EQUIP_GRADE Grade { get; private set; }
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


}
