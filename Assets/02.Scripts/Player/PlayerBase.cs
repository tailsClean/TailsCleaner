using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _attackInterval = 0.5f;
    [SerializeField] private BulletTest _bulletPrefab;


    private Vector2 _moveDir;
    private float _timer;


    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * _moveSpeed);

        _timer += Time.deltaTime;

        if(_timer > _attackInterval)
        {
            Attack();
            _timer -= _attackInterval;
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveDir = ctx.ReadValue<Vector2>().normalized;
    }

    public void Attack()
    {
        Vector2 dir = Mouse.current.position.ReadValue();
        Vector2 worldDir = Camera.main.ScreenToWorldPoint(dir).normalized;

        Vector2 spawnPos = (Vector2)transform.position + worldDir;
        BulletTest bullet = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);
        bullet.Spawn(worldDir);
    }
}
