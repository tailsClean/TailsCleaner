using UnityEngine;


public class RelicBase : ItemBase, IEnhancement
{
    public RelicData Data { get; private set; }

    public override void Init(int id) => Data = ItemDB.GetData<RelicData>(id);


    // 강화 데이터
    public int EnhanceLevel { get; private set; }
    public RelicEnhance EnhanceData => Data.Enhances[EnhanceLevel - 1];



    // 최종 스텟 증가량 제공 메서드(유물 증가량, 강화 증가량)
    public int GetIncreaseStat(STAT_TYPE stat)
    {
        float statValue = Data.Relic.stat_value;
        float enhanceValue = Data.Enhances[EnhanceLevel - 1].add_value;
        return (int)(statValue + enhanceValue);
    }

    public void SetEnhanceLevel(int level) => EnhanceLevel = level;

    public void OnEnhance(EnhancingInfo result) => EnhanceLevel++;
}
