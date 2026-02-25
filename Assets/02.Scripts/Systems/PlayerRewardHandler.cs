using UnityEngine;

public class PlayerRewardHandler : MonoBehaviour
{
    [Header("보상 데이터 (인스펙터 확인용)")]
    public RewardData myReward;


    void Update()
    {
        // 테스트용: 2번 키를 누르면 스테이지 승리 정산!
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("<color=orange>정산을 시작합니다!</color>");
            FinalizeReward(GameResult.Victory);
        }
    }

    private void Awake()
    {
        // 데이터 초기화
        myReward = new RewardData();
        myReward.Stage_ID = "Stage_01"; // 일단 임의 설정
        myReward.isFirstClear = true;
    }

    // 몬스터가 죽을 때 이 함수를 호출
    public void AddReward(int score, int gold)
    {
        myReward.score += score;
        myReward.goldEarned += gold;

        // 획득 예정 수치 로그 출력
        Debug.Log($"<color=cyan>[인게임 보상 누적]</color> 점수: {myReward.score}, 예상 골드: {myReward.goldEarned}");
    }

    // 외부에서 게임 종료 신호를 줄 때 호출
    public void FinalizeReward(GameResult result)
    {
        RewardManager.Instance.ProcessFinalReward(myReward, result);
    }
}