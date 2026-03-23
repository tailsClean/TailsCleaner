using UnityEditor;
using UnityEngine;

public abstract class PlayerState
{
    protected PlayerStateMachine _stateMachine;

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
    public virtual void HandleInput(PlayerInputData input) { }

    protected void RequestStateChange(State next)
    {
        if(_stateMachine == null) 
        { Debug.LogError("플레이어 상태에 상태머신 주입하기"); return; }

        _stateMachine.SetState(next);
    }



    public enum State
    {
        Idle, Move, Dead
    }
}

public class PlayerInputData
{
    public readonly Vector2 MoveDir;
    public readonly IRevive Revive;

    public PlayerInputData(Vector2 moveDir)
    {
        MoveDir = moveDir;
    }

    public PlayerInputData(IRevive revive)
    {
        Revive = revive;
    }

}