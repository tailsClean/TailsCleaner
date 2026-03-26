using UnityEngine;

public class AbandonState : IStageState
{
    private readonly StageController _controller;

    public AbandonState(StageController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[Stage] Enter AbandonState");

        // [중요]
        // 클리어 저장 없음
        // 보상 지급 없음
        // 필요하면 전용 UI를 나중에 붙일 수 있음
    }

    public void Tick(float deltaTime) { }
    public void Exit() { }
}