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
        Debug.Log("[Stage] Enter SuccessState");

        SaveStageClearProgress();
        GrantOutGameExp();
        ShowClearRewardUI();

        if (SoundManager.Instance) SoundManager.Instance.PlayStageResult(BGMName.Stage_Clear);
    }

    public void Exit()
    {
    }

    public void Tick(float _deltaTime)
    {
    }

    /// <summary>
    /// 스테이지 클리어 기록 저장
    /// </summary>
    private void SaveStageClearProgress()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SuccessState] GameManager.Instance is null.");
            return;
        }

        if (GameManager.Instance._currentStage == null)
        {
            Debug.LogWarning("[SuccessState] CurrentStage is null.");
            return;
        }

        StageTable currentStage = GameManager.Instance._currentStage;
        GameManager.Instance.MarkStageCleared(currentStage.tower_id, currentStage.stage_index);
    }

    /// <summary>
    /// 스테이지 클리어 시 아웃게임 경험치 지급
    /// </summary>
    private void GrantOutGameExp()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SuccessState] GameManager.Instance is null.");
            return;
        }

        if (GameManager.Instance._currentStage == null)
        {
            Debug.LogWarning("[SuccessState] CurrentStage is null.");
            return;
        }

        if (OutGameLevelSystem.Instance == null)
        {
            Debug.LogWarning("[SuccessState] OutGameLevelSystem.Instance is null.");
            return;
        }

        int expGain = Mathf.Max(0, GameManager.Instance._currentStage.exp_gain);

        if (expGain <= 0)
        {
            Debug.Log($"[SuccessState] No out-game exp to grant. stageId={GameManager.Instance._currentStage.stage_id}");
            return;
        }

        OutGameLevelSystem.Instance.GainExp(expGain);

        Debug.Log(
            $"[SuccessState] Granted OutGame Exp. " +
            $"stageId={GameManager.Instance._currentStage.stage_id}, expGain={expGain}"
        );
    }

    /// <summary>
    /// 클리어 UI 및 보상 UI 표시
    /// </summary>
    private void ShowClearRewardUI()
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[SuccessState] UIManager.Instance is null.");
            return;
        }

        UIManager.Instance.OpenClear();

        if (GameManager.Instance == null || GameManager.Instance._currentStage == null)
        {
            Debug.LogWarning("[SuccessState] Cannot show reward. CurrentStage is null.");
            return;
        }

        int rewardGroupId = GameManager.Instance._currentStage.reward_group_id;

        if (UIManager.Instance.ClearRewardUI != null)
        {
            UIManager.Instance.ClearRewardUI.ShowReward(rewardGroupId);
        }
        else
        {
            Debug.LogWarning("[SuccessState] ClearRewardUI is null.");
        }
    }
}