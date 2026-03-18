using UnityEngine;

public class OrbitProjectile : MonoBehaviour
{ 
    // 생성 시점에 Boss나 SpawnManager가 이 값을 세팅.
    private float finalDamage;

  
    public void SetDamage(float damage)
    {
        finalDamage = damage;
        Debug.Log("[BossMonster] 공전 패턴 시작");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 부모 또는 본인에게서 IDamageable을 찾음
            IDamageable target = collision.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                // 최종 데미지를 적용
                target.TakeDamage(finalDamage);

            }
        }
    }
}