using UnityEngine;

public class OrbitProjectile : MonoBehaviour
{
    public float damage;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IDamageable target = collision.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
     
            }
        }
    }

}