using System;
using UnityEngine;


// 강화 데이터 클래스
[Serializable]
public struct ItemEnhanceData
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public int Level { get; private set; }
    [field: SerializeField] public bool IsMaxLevel { get; private set; }
    [field: SerializeField] public float AddValue { get; private set; }
    [field: SerializeField] public int CostGold { get; private set; }
    [field: SerializeField] public int CostBluePrint { get; private set; }
    [field: SerializeField] public int BluePrintID { get; private set; }

    // 장착 장비의 강화 데이터 생성자
    public ItemEnhanceData(DefaultEquipData equip, int enhanceLevel)
    {
        var data = equip.Enhances[enhanceLevel];
        Debug.LogError("입력레벨" + enhanceLevel);
        Debug.LogError("강화레벨" +  data.level);
        Debug.LogError("강화맥스" +  data.is_max_level);
        ID = equip.Equipmnet.id;
        Level = enhanceLevel;
        IsMaxLevel = data.is_max_level;
        AddValue = data.add_value;
        CostGold = data.cost_gold;
        CostBluePrint = data.cost_blueprint;
        BluePrintID = data.blueprint_id;
    }

    // 유물의 강화 데이터 생성자
    public ItemEnhanceData(RelicData relic, int enhanceLevel)
    {
        var data = relic.Enhances[enhanceLevel];
        ID = relic.Relic.id;
        Level = enhanceLevel;
        IsMaxLevel = data.is_max_level;
        AddValue = data.add_value;
        CostGold = data.cost_gold;
        CostBluePrint = data.cost_fragment;
        BluePrintID = data.cost_id;
    }
}
