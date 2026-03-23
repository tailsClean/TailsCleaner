

public class IdleState : PlayerState
{
    private PlayerBase _player;

    public IdleState(PlayerBase player)
    {
        _player = player;
    }

    public override void Enter()
    {
        _player.PlayAni(PlayerAnimation.Idle);
    }
}