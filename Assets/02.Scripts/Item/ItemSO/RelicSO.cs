using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/Relic")]
public class RelicSO : PlayerEnhancementSO
{
    [Header("유물 정보")]
    public bool IsRelic;
    [SerializeField] private RELIC_STAT _statUp;
    [SerializeField] private int _statUpValue;
    [SerializeField] private RELIC_DIVISION _division;

    public RELIC_STAT StatUp => _statUp;
    public int StatUpValue => _statUpValue;
    public RELIC_DIVISION Division => _division;
}


public enum RELIC_STAT
{
    GoldGainRate,   // 골드 획득량 증가
    ItemDropRate,   // 아이템 획득 확률 증가
    ExpGainRate     // 경험치 획득량 증가
}

public enum RELIC_DIVISION
{
    Sparkle,        // 반짝반짝
    Smooth,         // 매끈매끈
    Swipe           // 쓱쓱싹싹
}
