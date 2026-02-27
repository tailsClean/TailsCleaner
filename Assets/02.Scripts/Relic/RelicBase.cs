using UnityEngine;


public class RelicBase : PlayerEnhancement
{
    [Header("유물 정보")]

    [field: SerializeField] public STAT StatUp { get; private set; }
    [field: SerializeField] public int StatUpValue { get; private set; }
    [field: SerializeField] public DIVISION Division { get; private set; }



    public enum STAT
    {
        GoldGain,        // 골드 획득량 증가
        ItemDropRate,    // 아이템 획득 확률 증가
        ExpGain          // 경험치 획득량 증가
    }

    public enum DIVISION
    {
        Sparkle,   // 반짝반짝
        Smooth,    // 매끈매끈
        Swipe      // 쓱쓱싹싹
    }
}
