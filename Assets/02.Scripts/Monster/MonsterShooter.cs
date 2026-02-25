using UnityEngine;

public class MonsterShooter : MonoBehaviour
{
    public GameObject projectilePrefab; // 탄환 프리팹
    public Transform firePoint;         // 탄환이 나갈 위치
    public Transform playerTarget;      // 플레이어 위치


    [Header("--- 공격 설정 ---")]
    public float fireRate = 2.0f;       // 2초마다 발사
    private float nextFireTime = 0f;    // 다음 총알이 나가기까지의 시간

    public void Update()
    {
        // 방법 1: 일정 시간마다 자동으로 Shoot() 호출
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        // 방법 2: 테스트를 위해 스페이스바를 누르면 발사
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Shoot();
        //}
    }

    public void Shoot()
    {
        // Debug.Log("1. Shoot 함수 진입 성공");

        if (projectilePrefab == null)
        {
            // Debug.LogError("에러: projectilePrefab(총알 프리팹)이 인스펙터에 연결 안 됨");
            return;
        }
        if (firePoint == null)
        {
            // Debug.LogError("에러: firePoint가 인스펙터에 연결 안 됨");
            return;
        }

        // Debug.Log("2. Instantiate(생성) 직전");

        // 생성 시도
        GameObject go = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        if (go != null)
        {
            // Debug.Log("3. 생성 성공! 이름: " + go.name);
        }
        else
        {
            // Debug.LogError("4. 생성 실패");
            return;
        }

        MonsterProjectile projectile = go.GetComponent<MonsterProjectile>();
        if (projectile != null)
        {
            // Debug.Log("5. MonsterProjectile 스크립트 발견, Launch 호출");
            projectile.Launch(playerTarget);
        }
        else
        {
            // Debug.LogError("6. 에러: 생성된 총알에 MonsterProjectile 스크립트가 없음");
        }
    }
}