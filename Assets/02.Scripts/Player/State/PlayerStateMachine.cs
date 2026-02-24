
using System.Collections.Generic;

public class PlayerStateMachine
{
    private Dictionary<State, IPlayerState> _stateDict;
    public IPlayerState CurrentState { get; private set; }

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

    public enum State
    {
        Idle, Move
    }
}
