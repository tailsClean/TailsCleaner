using UnityEngine;

public class EquipemtEnhance
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public int Level { get; private set; }
    public bool IsMaxLevel { get; private set; }
    public float AddValue { get; private set; }
    public int CostGold { get; private set; }
    public int CostBluePrint { get; private set; }
    public int BluePrintID { get; private set; }

    public EquipemtEnhance(EquipmentBase equip)
    {
        ID = equip.EnhanceID;
        GroupID = equip.EnhanceGroupID;
        Level = equip.EnhanceLevel;
        IsMaxLevel = equip.IsEnhanceMaxLevel;
        AddValue = equip.EnhanceAddValue;
        CostGold = equip.EnhanceCostGold;
        CostBluePrint = equip.EnhanceCostBluePrint;
        BluePrintID = equip.EnhanceBluePrintID;
    }
}