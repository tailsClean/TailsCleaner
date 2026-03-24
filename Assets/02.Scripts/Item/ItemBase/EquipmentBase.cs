using System;
using System.Collections.Generic;
using UnityEngine;


public class EquipmentBase : ItemBase
{
    public DefaultEquipData Data { get; private set; }
    public override void Init(int id)
    {
        if(ItemDB.TryGetData<DefaultEquipData>(id, out var data))
            Data = data;
    }


    // 강화 데이터
    public int CurrentEnhanceLevel { get; private set; } = 0;
    public EquipEnhance CurrentEnhanceData => Data.GetEnhance(CurrentEnhanceLevel);
    //{
    //    get
    //    {
    //        if (CurrentEnhanceLevel <= 0)
    //            CurrentEnhanceLevel = 1;

    //        return Data.GetEnhance(CurrentEnhanceLevel);
    //    }
    //}


    // 등급 데이터
    public GRADE CurrentGrade { get; private set; }
    public EquipGrade CurrentGradeData => Data.Grades[(int)CurrentGrade];





    // 특정 스텟 최종 증가량 제공 메서드(장비 증가량, 강화 증가량, 등급 증가량)
    public float GetIncreaseStat(EQUIP_STAT_TYPE stat)
    {
        
        float statValue = Data.Stat.TryGetValue(stat, out var data) ? data.value : 0;
        float enhanceValue = CurrentEnhanceData != null ? CurrentEnhanceData.add_value : 0;
        float gradeValue = CurrentGradeData.stat_rate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return result;
    }

    public void OnEnhance(EnhancingInfo result) => CurrentEnhanceLevel = result.CurrentEnhanceLevel;
    public void OnUpgrade() => CurrentGrade++;

}
