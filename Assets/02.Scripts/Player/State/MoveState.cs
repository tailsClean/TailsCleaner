using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerBase _player;

    public Vector2 MoveDir {  get; private set; }
    

    public MoveState(PlayerBase player)
    {
        _player = player;
    }

    public void Enter()
    {
    }

    public void Exit()
    {
    }

    public void Update()
    {
    }

    //public void OnMove(InputAction.CallbackContext ctx)
    //{
    //    Vector2 dir = ctx.ReadValue<Vector2>();

    //    _moveDir = dir.normalized;
    //    if (_moveDir.x < 0)
    //        transform.localScale = new Vector3(-1, 1, 1);
    //    else if (_moveDir.x > 0)
    //        transform.localScale = new Vector3(1, 1, 1);
    //}

    public void SetDirection(Vector2 direction) => MoveDir = direction;
}