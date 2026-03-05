using UnityEngine;


public class RelicBase : PlayerEnhancement
{
    [Header("유물 정보")]
    public bool IsRelic;
    [field: SerializeField] public STAT StatUp { get; private set; }
    [field: SerializeField] public int StatUpValue { get; private set; }
    [field: SerializeField] public DIVISION Division { get; private set; }


    // 최종 스텟 증가량 제공 메서드(유물 증가량, 강화 증가량)
    public int GetIncreaseStat()
    {
        float statValue = StatUpValue;
        float enhanceValue = EquipEnhance.AddValue;
        return (int)(statValue + enhanceValue);
    }

    public override Sprite GetSprite() => SpriteImage;      // 수정 필요

    public enum STAT
    {
        GoldGainRate,   // 골드 획득량 증가
        ItemDropRate,   // 아이템 획득 확률 증가
        ExpGainRate     // 경험치 획득량 증가
    }

    public enum DIVISION
    {
        Sparkle,   // 반짝반짝
        Smooth,    // 매끈매끈
        Swipe      // 쓱쓱싹싹
    }
}
