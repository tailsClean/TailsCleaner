using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private BulletTest _bulletPrefab;


    private Vector2 _moveDir;

    public void OnMove(InputAction.CallbackContext ctx)
    {
        _moveDir = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        Vector3 dir = new Vector3(_moveDir.x, 0, _moveDir.y);
        transform.Translate(dir.normalized * Time.deltaTime * _moveSpeed);
;    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        Vector3 dir = Mouse.current.position.ReadValue();
    }
}
