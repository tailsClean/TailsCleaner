using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 결과 화면 진입 시 호출
    public void ProcessFinalReward(RewardData data, GameResult result)
    {
        //--------------------------------
        // 랭크 계산
        //--------------------------------
        if (result == GameResult.GiveUp)
        {
            data.rank = "F";
            data.multiplier = 0f;
            data.Coin = 0;
        }
        else
        {
            CalculateRank(data);
        }

        //--------------------------------
        // 승리 / 패배 / 포기 비율
        //--------------------------------
        float resultRatio = 0f;

        if (result == GameResult.Victory)
            resultRatio = 1.0f;
        else if (result == GameResult.Defeat)
            resultRatio = 0.7f;
        else if (result == GameResult.GiveUp)
            resultRatio = 0f;

        //--------------------------------
        // 최종 골드 계산
        //--------------------------------
        int finalGold = Mathf.FloorToInt(
            (data.goldEarned * data.multiplier) * resultRatio
        );

        //--------------------------------
        // 최초 클리어 보상
        //--------------------------------
        if (data.isFirstClear && result == GameResult.Victory)
        {
            GiveFirstClearReward(data.Stage_ID);
        }

        //--------------------------------
        // 인벤토리 지급
        //--------------------------------
        SendToInventory(finalGold, data.Coin);
    }

    //--------------------------------
    // 점수 기반 랭크 계산
    //--------------------------------
    private void CalculateRank(RewardData data)
    {
        if (data.score >= 10000)
        {
            data.rank = "S";
            data.multiplier = 1.2f;
            data.Coin = 20;
        }
        else if (data.score >= 7000)
        {
            data.rank = "A";
            data.multiplier = 1.1f;
            data.Coin = 15;
        }
        else if (data.score >= 1)
        {
            data.rank = "B";
            data.multiplier = 1.0f;
            data.Coin = 10;
        }
        else
        {
            data.rank = "F";
            data.multiplier = 0f;
            data.Coin = 0;
        }
    }

    //--------------------------------
    // 최초 클리어 보상
    //--------------------------------
    private void GiveFirstClearReward(string stageID)
    {
        Debug.Log($"{stageID} 최초 클리어 고정 보상 지급");
    }

    //--------------------------------
    // 인벤토리 지급 (서버 연동 지점)
    //--------------------------------
    private void SendToInventory(int finalGold, int finalCoin)
    {
        Debug.Log($"인벤토리 지급 - 골드: {finalGold}, 코인: {finalCoin}");
    }
}