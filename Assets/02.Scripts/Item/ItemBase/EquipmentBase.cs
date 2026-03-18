using System.Collections.Generic;
using UnityEngine;


public class EquipmentBase : ItemBase, IEnhancement
{
    public EquipData Data { get; private set; }
    public override void Init(int id) => Data = ItemDB.GetData<EquipData>(id);


    // 강화 데이터
    public int EnhanceLevel { get; private set; } = 1;
    public EquipEnhance EnhanceData => Data.Enhances[EnhanceLevel - 1];

    // 등급 데이터
    public GRADE Grade { get; private set; }
    public EquipGrade GradeData => Data.Grades[(int)Grade];





    // 특정 스텟 최종 증가량 제공 메서드(장비 증가량, 강화 증가량, 등급 증가량)
    public float GetIncreaseStat(EQUIP_STAT_TYPE stat)
    {
        
        float statValue = Data.Stat.TryGetValue(stat, out var data) ? data.value : 0;
        float enhanceValue = EnhanceData.add_value;
        float gradeValue = GradeData.stat_rate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return result;
    }

    public void OnEnhance(EnhancingInfo result) => EnhanceLevel = result.EnhanceLevel;
    public void OnUpgrade() => Grade++;

}
