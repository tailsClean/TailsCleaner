using UnityEngine;


public class RelicBase : ItemBase, IEnhancement
{
    public RelicSO Data { get; private set; }

    public override void Init(int id) => Data = ItemDB.GetItemSO<RelicSO>(id);


    // 강화 데이터
    public int EnhanceLevel { get; private set; }
    public EnhanceData EnhanceData => Data.GetEnhanceData(EnhanceLevel);


    // 최종 스텟 증가량 제공 메서드(유물 증가량, 강화 증가량)
    public int GetIncreaseStat(RELIC_STAT stat)
    {
        float statValue = Data.GetIncreaseStat();
        float enhanceValue = EnhanceData.AddValue;
        return (int)(statValue + enhanceValue);
    }
}
