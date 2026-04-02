using UnityEngine;

public class LaserBase : MonoBehaviour
{
    [Header("데미지를 주는 레이저일 경우 체크")]
    [SerializeField] private bool _isAttackable;

    private IInvincible _player;
    private float _firstDamage;
    private float _damage;
    private bool _isFirstDamaged;
    private bool _isPlayerStayed;

    private void OnEnable()
    {
        _isFirstDamaged = true;
    }

    private void OnDisable()
    {
        _isAttackable = false;
        _isPlayerStayed = false;
    }

    private void Update()
    {
        if (!_isPlayerStayed)
            return;

        OnTakeDamage();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CheckAttackablePlayer(collision))
            return;

        _isPlayerStayed = true;
        if(collision.TryGetComponent<IInvincible>(out var player))
        {
            _player = player;
            _player.SetInvincibleTime(1f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!CheckAttackablePlayer(collision))
            return;

        _isPlayerStayed = false;
        _player.SetInvincibleTime(PlayerHpSystem.INVINCIBLE_TIME);
    }



    public void Init(float firstDamage, float damage)
    {
        if (_isAttackable)
        {
            _firstDamage = firstDamage;
            _damage = damage;
            _isFirstDamaged = false;
            _isPlayerStayed = false;
        }
    }

    #region 내부 메서드

    private bool CheckAttackablePlayer(Collider2D collision) =>
        _isAttackable && collision.CompareTag("Player");

    private void OnTakeDamage()
    {
        if (_isFirstDamaged)
        {
            _player.TakeDamage(_firstDamage);
            _isFirstDamaged= false;
        }

        else
            _player.TakeDamage(_damage);
    }

    #endregion
}
