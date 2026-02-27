using UnityEngine;
using System.Collections.Generic;

public class PlayerStateMachine
{
    private Dictionary<State, IPlayerState> _stateDict;
    public IPlayerState CurrentState { get; private set; }

    public Vector2 MoveDir { get; private set; }

    public PlayerStateMachine(PlayerBase player)
    {
        _stateDict = new Dictionary<State, IPlayerState>();
        _stateDict.Add(State.Idle, new IdleState(player));
        _stateDict.Add(State.Move, new MoveState(player));
        CurrentState = _stateDict[State.Idle];
    }

    public void SetState(State nextState)
    {
        CurrentState?.Exit();
        CurrentState = _stateDict[nextState];
        CurrentState?.Enter();
    }

    public void Update() => CurrentState.Update();

    public void MoveInput(Vector2 dir)
    {
        MoveDir = dir;
        Debug.Log(_stateDict[State.Move]);
        _stateDict[State.Move].HandleInput(new PlayerInputData(MoveDir));
        SetState(State.Move);

        if(dir == Vector2.zero)
            SetState(State.Idle);
    }

    public enum State
    {
        Idle, Move
    }
}
