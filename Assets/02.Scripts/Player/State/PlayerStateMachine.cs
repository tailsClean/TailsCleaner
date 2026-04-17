using UnityEngine;
using System.Collections.Generic;
using static PlayerState;

public class PlayerStateMachine
{
    private Dictionary<State, PlayerState> _stateDict;
    public PlayerState CurrentState { get; private set; }

    public Vector2 MoveDir { get; private set; }

    public PlayerStateMachine(PlayerBase player, PlayerAnimation ani)
    {
        _stateDict = new Dictionary<State, PlayerState>();
        _stateDict.Add(State.Idle, new IdleState(player));
        _stateDict.Add(State.Move, new MoveState(player));
        _stateDict.Add(State.Dead, new DeadState(player, ani, this));
        CurrentState = _stateDict[State.Idle];
    }

    public void SetState(State nextState)
    {
        CurrentState?.Exit();
        CurrentState = _stateDict[nextState];
        CurrentState?.Enter();
    }

    public void Update() => CurrentState.Tick();

    // Move 상태에 이동방향을 주입
    public void MoveInput(Vector2 dir)
    {
        MoveDir = dir;
        _stateDict[State.Move].HandleInput(new PlayerInputData(MoveDir));
        SetState(State.Move);

        if(dir == Vector2.zero)
            SetState(State.Idle);
    }

    public void DeadInput(IRevive revive)
    {
        _stateDict[State.Dead].HandleInput(new PlayerInputData(revive));
        SetState(State.Dead);
    }


}
