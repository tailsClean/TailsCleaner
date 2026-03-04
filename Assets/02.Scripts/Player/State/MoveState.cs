using UnityEngine;

public class MoveState : IPlayerState
{
    private PlayerBase _player;
    private float _moveSpeed;
    private Vector2 _moveDir;
    

    public MoveState(PlayerBase player)
    {
        _player = player;
        _moveSpeed = player.MoveSpeed;
    }

    public void Enter() { }

    public void Exit() { }

    public void Update()
    {
        OnMove();
    }

    public void HandleInput(PlayerInputData input)
    {
        _moveDir = input.MoveDir;
    }



    private void OnMove()
    {
        _player.transform.Translate(_moveDir * Time.deltaTime * _moveSpeed);

        ChangeFlip();
    }

    private void ChangeFlip()
    {
        if (_moveDir.x < 0)
            _player.transform.localScale = new Vector3(-1, 1, 1);
        else if (_moveDir.x > 0)
            _player.transform.localScale = new Vector3(1, 1, 1);
    }


}