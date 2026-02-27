using UnityEngine;

public class EnhanceSystem
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public int Level { get; private set; }
    public bool IsMaxLevel { get; private set; }
    public float AddValue { get; private set; }
    public int CostGold { get; private set; }
    public int CostBluePrint { get; private set; }
    public int BluePrintID { get; private set; }

    public EnhanceSystem(PlayerEnhancement enhancement)
    {
        ID = enhancement.EnhanceID;
        GroupID = enhancement.EnhanceGroupID;
        Level = enhancement.EnhanceLevel;
        IsMaxLevel = enhancement.IsEnhanceMaxLevel;
        AddValue = enhancement.EnhanceAddValue;
        CostGold = enhancement.EnhanceCostGold;
        CostBluePrint = enhancement.EnhanceCostBluePrint;
        BluePrintID = enhancement.EnhanceBluePrintID;
    }
}