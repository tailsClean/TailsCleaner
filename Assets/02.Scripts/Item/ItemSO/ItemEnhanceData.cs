using System;
using UnityEngine;


// 강화 데이터 클래스
[Serializable]
public struct ItemEnhanceData
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    //[field: SerializeField] public bool IsMaxLevel { get; private set; }
    [field: SerializeField] public float AddValue { get; private set; }
    [field: SerializeField] public int CostGold { get; private set; }
    [field: SerializeField] public int CostBluePrint { get; private set; }
    [field: SerializeField] public int BluePrintID { get; private set; }

    // 장착 장비의 강화 데이터 생성자
    public ItemEnhanceData(DefaultEquipData equip, int nextEnhanceLevel)
    {
        var data = equip.GetEnhance(nextEnhanceLevel);
        ID = equip.Equipmnet.id;
        Level = nextEnhanceLevel;
        //IsMaxLevel = data.is_max_level;
        AddValue = data.add_value;
        CostGold = data.cost_gold;
        CostBluePrint = data.cost_blueprint;
        BluePrintID = data.blueprint_id;
    }

    // 유물의 강화 데이터 생성자
    public ItemEnhanceData(RelicData relic, int nextEnhanceLevel)
    {
        var data = relic.GetEnhance(nextEnhanceLevel);
        ID = relic.Relic.id;
        Level = nextEnhanceLevel;
        //IsMaxLevel = data.is_max_level;
        AddValue = data.add_value;
        CostGold = data.cost_gold;
        CostBluePrint = data.cost_fragment;
        BluePrintID = data.cost_id;
    }
}
