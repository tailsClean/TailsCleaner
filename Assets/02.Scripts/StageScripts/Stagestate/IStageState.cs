using UnityEngine;

public interface IStageState
{
    void Enter();
    void Tick(float _deltaTime);
    void Exit();
}
