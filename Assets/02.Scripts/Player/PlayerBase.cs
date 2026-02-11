using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable
{
    [SerializeField] private float _hp = 100;

    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _attackInterval = 0.5f;      // 자동공격 주기
    [SerializeField] private GameObject _bulletPrefab;


    private Vector2 _moveDir;
    private SpriteRenderer _mySprite;
    private bool _isInvincible;                                 // 피격 무적상태 여부
    private float _timer;

    public float Hp => Mathf.Max(_hp, 0);
    public bool IsInvincible => _isInvincible;


    private void Awake()
    {
        _mySprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * _moveSpeed);

        _timer += Time.deltaTime;

        if(_timer > _attackInterval)
        {
            OnAttack();
            _timer -= _attackInterval;
        }
    }


    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveDir = ctx.ReadValue<Vector2>().normalized;
    }

    // 공격 기능
    public void OnAttack()
    {
        if (!_bulletPrefab)
            return;

        // 마우스 위치의 벡터값
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 attackDir = Camera.main.ScreenToWorldPoint(mousePos) - transform.position;

        Vector2 spawnPos = (Vector2)transform.position + attackDir.normalized;
        var obj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);

         if (obj.TryGetComponent<IBullet>(out var bullet))
            bullet.SetDirection(attackDir.normalized);
    }

    // 피격시, 발동되는 메서드
    public void TakeDamage(float damage)
    {
        if (_isInvincible)
            return;

        _hp -= damage;
        Debug.Log("현재 체력: " + Hp);

        StartCoroutine(StartHitInvincibility());
    }

    // 피격 시, 무적 + 피격이펙트(깜빡임)
    private IEnumerator StartHitInvincibility()
    {
        _isInvincible = true;

        // 깜빡이는 메서드
        var wait = new WaitForSeconds(0.2f);
        Color original = _mySprite.color;
        for(int i = 0; i < 5; i++)
        {
            _mySprite.color = new Color(0, 0, 0);
            yield return wait;

            _mySprite.color = original;
            yield return wait;
        }

        _isInvincible = false;
    }
}
