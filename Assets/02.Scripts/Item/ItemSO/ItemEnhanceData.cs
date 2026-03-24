using System;
using UnityEngine;


// 강화 데이터 클래스
[Serializable]
public struct ItemEnhanceData
{
    public int CostGold { get; private set; }
    public int CostBluePrint { get; private set; }
    public int BluePrintID { get; private set; }

    // 장착 장비의 강화 데이터 생성자
    public ItemEnhanceData(DefaultEquipData equip, int nextEnhanceLevel)
    {

        var data = equip.GetEnhance(nextEnhanceLevel);
        if (data == null)
        {
            CostGold = 0;
            CostBluePrint = 0;
            BluePrintID = 0;
            return;
        }

        CostGold = data.cost_gold;
        CostBluePrint = data.cost_blueprint;
        BluePrintID = data.blueprint_id;
    }

    // 유물의 강화 데이터 생성자
    public ItemEnhanceData(RelicData relic, int nextEnhanceLevel)
    {
        var data = relic.GetEnhance(nextEnhanceLevel);
        if (data == null)
        {
            CostGold = 0;
            CostBluePrint = 0;
            BluePrintID = 0;
            return;
        }

        CostGold = data.cost_gold;
        CostBluePrint = data.cost_fragment;
        BluePrintID = data.cost_id;
    }
}
