using UnityEngine;

public class FailState : IStageState
{
     private readonly StageController _controller;
    public StageFailReason Reason { get; }

    public FailState(StageController controller, StageFailReason reason)
    {
        _controller = controller;
        Reason = reason;
    }

    public void Enter()
    {
        Debug.Log($"[Stage] Enter FailState reason={Reason} -> FinalizeReward(Defeat)");
        _controller?.RewardHandler?.FinalizeReward(GameResult.Defeat);
    }

    public void Tick(float _deltaTime) { }
    public void Exit() { }
}
