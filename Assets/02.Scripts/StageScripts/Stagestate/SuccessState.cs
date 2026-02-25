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
        _controller?.RewardHandler?.FinalizeReward(GameResult.Victory);
    }

    public void Exit()
    { }

    public void Tick(float _deltaTime)
    { }
}
