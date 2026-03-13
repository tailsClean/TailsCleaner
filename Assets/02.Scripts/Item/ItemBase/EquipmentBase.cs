using System.Collections.Generic;
using UnityEngine;


public class EquipmentBase : ItemBase, IEnhancement
{
    public EquipmentSO Data { get; private set; }
    public override void Init(int id) => Data = ItemDB.GetItemData<EquipmentSO>(id);


    public EQUIP_PARTS EquipmentPart => Data.EquipmentPart;

    // 강화 데이터
    public int EnhanceLevel { get; private set; }
    public ItemBaseSO ItemData => Data;
    public ItemEnhanceData EnhanceData => Data.GetEnhanceData(EnhanceLevel + 1);

    // 등급 데이터
    public EQUIP_GRADE Grade { get; private set; }
    public EquipmentSO.EquipGradeData GradeData => Data.GetGradeData(Grade);





    // 특정 스텟 최종 증가량 제공 메서드(장비 증가량, 강화 증가량, 등급 증가량)
    public float GetIncreaseStat(EQUIP_STAT stat)
    {
        
        float statValue = Data.GetIncreaseStat(stat);
        float enhanceValue = EnhanceData.AddValue;
        float gradeValue = GradeData.StatRate;
        float result = statValue * (1 + enhanceValue) * gradeValue;
        return result;
    }

    public void OnEnhance(EnhancingInfo result) => EnhanceLevel = result.EnhanceLevel;
    public void OnUpgrade() => Grade++;

}
