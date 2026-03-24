using System;
using UnityEngine;


public class RelicBase : ItemBase
{
    public RelicData Data { get; private set; }

    public override void Init(int id)
    {
        if (ItemDB.TryGetData<RelicData>(id, out var data))
            Data = data;
    }


    // 강화 데이터
    public int CurrentEnhanceLevel { get; private set; } = 0;
    public RelicEnhance CurrentEnhanceData => Data.GetEnhance(CurrentEnhanceLevel);
    //{
    //    get
    //    {
    //        if(CurrentEnhanceLevel <= 0)
    //            CurrentEnhanceLevel = 1;

    //        return Data.GetEnhance(CurrentEnhanceLevel);
    //    }
    //}


    // 최종 스텟 증가량 제공 메서드(유물 증가량, 강화 증가량)
    public int GetIncreaseStat(STAT_TYPE stat)
    {
        float statValue = Data.Relic.stat_value;
        float enhanceValue = CurrentEnhanceData != null ? CurrentEnhanceData.add_value : 0;
        return (int)(statValue + enhanceValue);
    }

    public void SetEnhanceLevel(int level) => CurrentEnhanceLevel = level;

    public void OnEnhance(EnhancingInfo result) => CurrentEnhanceLevel++;
}
