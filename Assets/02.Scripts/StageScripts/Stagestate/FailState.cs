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
        Debug.Log($"[Stage] Enter FailState reason={Reason}");
        UIManager.Instance.OpenGameOver();

        if (SoundManager.Instance) SoundManager.Instance.PlayStageResult(BGMName.Stage_Clear);
    }

    public void Tick(float _deltaTime) { }
    public void Exit() { }
}