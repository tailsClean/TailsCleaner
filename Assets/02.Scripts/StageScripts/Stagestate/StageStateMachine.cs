using UnityEngine;

public class StageStateMachine
{
    private IStageState _currentState;

    public void ChangeState(IStageState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Tick(float _deltaTime)
    {
        _currentState?.Tick(_deltaTime);
    }
}
