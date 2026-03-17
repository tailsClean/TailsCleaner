using System;
using UnityEngine;


// 강화 데이터 클래스
[Serializable]
public class ItemEnhanceData
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int GroupID { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public bool IsMaxLevel { get; private set; }
    [field: SerializeField] public float AddValue { get; private set; }
    [field: SerializeField] public int CostGold { get; private set; }
    [field: SerializeField] public int CostBluePrint { get; private set; }
    [field: SerializeField] public int BluePrintID { get; private set; }

    public ItemEnhanceData(EquipEnhance enhance)
    {
        ID = enhance.id;
        GroupID = enhance.group_id;
        Level = enhance.level;
        IsMaxLevel = enhance.is_max_level;
        AddValue = enhance.add_value;
        CostGold = enhance.cost_gold;
        CostBluePrint = enhance.cost_blueprint;
        BluePrintID = enhance.blueprint_id;
    }
}
