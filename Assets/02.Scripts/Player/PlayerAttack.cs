using UnityEngine;


public class PlayerAttack 
{
    private PlayerBase _player;
    private Bullet _bullet;
    private float _attackInterval;
    private Vector2 _attackDir;
    private float _timer;

    public PlayerAttack(PlayerBase player, Bullet bullet, float attackInterval)
    {
        _player = player;
        _bullet = bullet;
        _attackInterval = attackInterval;
        _attackDir = _attackDir = new Vector2(0, -player.transform.localScale.y);
    }


    public void OnAttack()
    {
        _timer += Time.deltaTime;

        if (_timer > _attackInterval)
        {
            FireBullet();
            _timer -= _attackInterval;
        }
    }

    private void FireBullet()
    {
        if(_bullet == null)
        {
            Debug.LogWarning("불릿 프리팹이 없습니다.");
            return;
        }

        _attackDir = _player.AttackDir.normalized;

        Vector2 spawnPos = (Vector2)_player.transform.position + _attackDir;
        var bullet = _player.FireBullet(_bullet, spawnPos);

        bullet.SetDirection(_attackDir.normalized);
    }
}