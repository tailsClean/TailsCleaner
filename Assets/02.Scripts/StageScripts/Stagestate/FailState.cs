using UnityEngine;

public class FailState : IStageState
{
    public StageFailReason Reason { get; }

    public FailState(StageFailReason reason)
    {
        Reason = reason;
    }

    public void Enter() { }
    public void Tick(float _deltaTime) { }
    public void Exit() { }
}
