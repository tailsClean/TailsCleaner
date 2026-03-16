using UnityEngine;

public class ZoneSpawner : MonoBehaviour
{
    public LayerMask zoneLayer;
    public int maxSpawnAttempts = 10;

    public void SpawnZone(GameObject zonePrefab, Transform target, int count, float range, bool isSafeZone, float radius)
    {
        // 프리팹에서 PoolObject 컴포넌트를 미리 확인
        PoolObject poolPrefab = zonePrefab.GetComponent<PoolObject>();

        if (poolPrefab == null)
        {
            Debug.LogError($"{zonePrefab.name} 프리팹에 PoolObject 스크립트가 없습니다! 풀링을 사용할 수 없습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            bool success = false;
            Vector3 spawnPos = Vector3.zero;

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                Vector2 randomCircle = Random.insideUnitCircle * range;
                spawnPos = new Vector3(target.position.x + randomCircle.x, target.position.y + randomCircle.y, 0);

                if (isSafeZone)
                {
                    // 레이어 마스크를 이용해 해당 위치에 이미 안전 지대가 있는지 체크
                    Collider2D hit = Physics2D.OverlapCircle(spawnPos, radius, zoneLayer);
                    if (hit == null) { success = true; break; }
                }
                else { success = true; break; }
            }

            if (success)
            {
                // Instantiate 대신 ObjectPoolManager를 통해 소환
                PoolObject spawnedObj = ObjectPoolManager.Instance.Spawn(poolPrefab, spawnPos, Quaternion.identity);

                // 소환된 객체에서 AreaEffector를 찾아 반지름(radius)을 설정합니다.
                if (spawnedObj.TryGetComponent<AreaEffector>(out var effector))
                {
                    effector.radius = radius;
                }

                // 안전 지대일 경우 레이어를 설정하여 중복 생성을 방지
                if (isSafeZone)
                {
                    spawnedObj.gameObject.layer = GetLayerFromMask(zoneLayer);
                }
            }
        }
    }

    private int GetLayerFromMask(LayerMask mask)
    {
        int bitmask = mask.value;
        if (bitmask == 0) return 0;
        for (int i = 0; i < 32; i++)
        {
            if (((1 << i) & bitmask) != 0) return i;
        }
        return 0;
    }
}