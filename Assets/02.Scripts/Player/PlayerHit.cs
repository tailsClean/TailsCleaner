using System.Collections;
using UnityEngine;

public class PlayerHit
{
    private PlayerBase _player;
    private SpriteRenderer _playerSprite;
    private bool _isInvincible;                     // 피격시, 잠시 무적(true)이 됨


    public PlayerHit(PlayerBase player)
    {
        _player = player;
        _playerSprite = player.GetComponent<SpriteRenderer>();
    }


    public int TakeDamage(int hp, int damage)
    {
        if(!_isInvincible)
        {
            hp -= damage;
            Debug.Log("피격");
            _player.StartCoroutine(StartHitInvincibility());
        }
        return hp;
    }

    // 피격 시, 무적 + 피격이펙트(깜빡임)
    private IEnumerator StartHitInvincibility()
    {
        _isInvincible = true;

        // 깜빡이는 메서드
        var wait = new WaitForSeconds(0.2f);
        Color original = _playerSprite.color;
        for (int i = 0; i < 3; i++)
        {
            _playerSprite.color = new Color(0, 0, 0);
            yield return wait;

            _playerSprite.color = original;
            yield return wait;
        }

        _isInvincible = false;
    }
}

