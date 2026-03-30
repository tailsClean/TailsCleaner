using UnityEngine;

public class LaserBase : MonoBehaviour
{
    [Header("데미지를 주는 레이저일 경우 체크")]
    [SerializeField] private bool _isAttackable;

    private float _damage;


    private void OnDisable()
    {
        _isAttackable = false;
    }


    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!_isAttackable)
            return;

        if (!collision.CompareTag("Player"))
            return;

        if (collision.TryGetComponent<IDamageable>(out var player))
            player.TakeDamage(_damage);
    }

    public void Init(float att)
    {
        if(_isAttackable)
            _damage = att;
    }
}
