using UnityEngine;

public interface IPlayerState
{
    void Enter();
    void Exit();
    void Update();
    void HandleInput(PlayerInputData input);

}

public struct PlayerInputData
{
    public readonly Vector2 MoveDir;

    public PlayerInputData(Vector2 moveDir)
    {
        MoveDir = moveDir;
    }
}