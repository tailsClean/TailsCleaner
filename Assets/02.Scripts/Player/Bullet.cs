using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5;
    private float _attackDamage;

    private Vector2 _dir;

    void Update()
    {
        transform.Translate(_dir *  Time.deltaTime * _moveSpeed);
        Destroy(gameObject, 2f);
    }

    public void Init(Vector3 dir, int attackDamage)
    {
        _dir = dir.normalized;
        _attackDamage = attackDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            var player = collision.GetComponent<IDamageable>();

            if (player != null)
            {
                player.TakeDamage(_attackDamage);
                Destroy(gameObject);
            }
        }
    }
}