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

        Debug.Log("[Stage] Enter SuccessState -> FinalizeReward(Victory)");
        UIManager.Instance.OpenClear();
        _controller?.RewardHandler?.FinalizeReward(GameResult.Victory);
    }

    public void Exit()
    { }

    public void Tick(float _deltaTime)
    { }

    private void SaveStageClearProgress()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance._currentStage == null) return;

        StageTable currentStage = GameManager.Instance._currentStage;
        // RewardManager.Instance.SendToInventory(); 대충 이런거 보내야함.
        GameManager.Instance.MarkStageCleared(currentStage.tower_id, currentStage.stage_index);
    }
}
