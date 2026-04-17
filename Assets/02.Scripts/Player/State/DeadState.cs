

using System.Collections;
using UnityEngine;

public class DeadState : PlayerState
{
    private PlayerBase _player;
    private PlayerAnimation _aniSystem;
    private IRevive _revive;
    private float _deadDelay = 4f;
    private int _reviveCount = 1;           // 부활 카운트

    public DeadState(
        PlayerBase player, 
        PlayerAnimation playerAni, 
        PlayerStateMachine stateMachine)
    {
        _stateMachine = stateMachine;
        _player = player;
        _aniSystem = playerAni;
    }

    public override void Enter()
    {
        _player.PlayAni(PlayerAnimation.Dead);
        _reviveCount--;

        if (_reviveCount > 0)
            _player.StartCoroutine(ResetHp());
    }

    public override void HandleInput(PlayerInputData input)
    {
        _revive = input.Revive;
    }

    private IEnumerator ResetHp()
    {
        yield return null;

        yield return new WaitForSeconds(_aniSystem.CurrentAniTime + _deadDelay);

        _revive.OnRevive();
        RequestStateChange(State.Idle);
    }
}