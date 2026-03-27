using UnityEngine;

public class SuccessState : IStageState
{
    private readonly StageController _controller;

    public SuccessState(StageController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        SaveStageClearProgress();

        Debug.Log("[Stage] Enter SuccessState");

        // [수정] stage_id가 아니라 reward_group_id 전달
        UIManager.Instance.OpenClear();

        if (GameManager.Instance != null && GameManager.Instance._currentStage != null)
        {
            int rewardGroupId = GameManager.Instance._currentStage.reward_group_id;

            if (UIManager.Instance != null && UIManager.Instance.ClearRewardUI != null)
            {
                UIManager.Instance.ClearRewardUI.ShowReward(rewardGroupId);
            }
            else
            {
                Debug.LogWarning("[SuccessState] ClearRewardUI is null.");
            }
        }
    }

    public void Exit()
    {
    }

    public void Tick(float _deltaTime)
    {
    }

    private void SaveStageClearProgress()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance._currentStage == null) return;

        StageTable currentStage = GameManager.Instance._currentStage;
        GameManager.Instance.MarkStageCleared(currentStage.tower_id, currentStage.stage_index);
    }
}