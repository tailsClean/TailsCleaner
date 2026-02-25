using UnityEngine;

public enum GameResult { Victory, Defeat, GiveUp }

[System.Serializable]
public class RewardData
{
    public string Stage_ID;
    public bool isFirstClear;

    public int score;           // 점수
    public int goldEarned;      // 스테이지 내에서 획득한 기본 골드

    public string rank;         // S, A, B, F 랭크
    public float multiplier;    // 1.0 ~ 1.2배 계수
    public int Coin;    // 교환 전용 재화(보너스 코인)
}
