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
        Debug.Log("[Stage] Enter SuccessState -> FinalizeReward(Victory)");
        
        SaveStageClearProgress();

        UIManager.Instance.OpenClear();
        _controller?.RewardHandler?.FinalizeReward(GameResult.Victory);
    }

    public void Exit()
    { }

    public void Tick(float _deltaTime)
    { }

    private void SaveStageClearProgress()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[SuccessState] GameManager is null. Clear progress not saved.");
            return;
        }

        StageTable currentStage = GameManager.Instance._currentStage;
        if (currentStage == null)
        {
            Debug.LogWarning("[SuccessState] CurrentStage is null. Clear progress not saved.");
            return;
        }

        GameManager.Instance.MarkStageCleared(currentStage.tower_id, currentStage.stage_index);

        Debug.Log($"[SuccessState] Stage clear saved. towerId={currentStage.tower_id}, stageIndex={currentStage.stage_index}");
    }
}
