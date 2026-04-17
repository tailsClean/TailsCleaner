using UnityEngine;

public class ZoneSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public LayerMask safeZoneLayer;
    public int maxSpawnAttempts = 15;

    [Header("Default Prefabs")]
    public GameObject defaultDangerZonePrefab;
    public GameObject defaultSafeZonePrefab;

    public void SpawnDangerZones(
        GameObject zonePrefab,
        Transform target,
        int count,
        float range,
        float radius,
        float previewTime,
        float activeTime,
        float damagePerTick,
        float tickInterval)
    {
        if (zonePrefab == null)
            zonePrefab = defaultDangerZonePrefab;

        if (zonePrefab == null || target == null)
            return;

        PoolObject poolPrefab = zonePrefab.GetComponent<PoolObject>();
        if (poolPrefab == null)
        {
            Debug.LogError($"{zonePrefab.name} 프리팹에 PoolObject가 없습니다.");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 spawnPos = GetRandomPointAroundTarget(target.position, range);

            PoolObject spawned = ObjectPoolManager.Instance.Spawn(poolPrefab, spawnPos, Quaternion.identity);
            if (spawned == null) continue;

            DangerZone zone = spawned.GetComponent<DangerZone>();
            if (zone != null)
            {
                zone.InitializeDanger(radius, previewTime, activeTime, damagePerTick, tickInterval);
            }
        }
    }

    public void SpawnSafeZones(
        GameObject zonePrefab,
        Transform target,
        int count,
        float range,
        float radius,
        float previewTime,
        float activeTime)
    {
        if (zonePrefab == null)
            zonePrefab = defaultSafeZonePrefab;

        if (zonePrefab == null || target == null)
            return;

        PoolObject poolPrefab = zonePrefab.GetComponent<PoolObject>();
        if (poolPrefab == null)
        {
            Debug.LogError($"{zonePrefab.name} 프리팹에 PoolObject가 없습니다.");
            return;
        }

        int safeLayer = GetLayerFromMask(safeZoneLayer);

        for (int i = 0; i < count; i++)
        {
            bool success = false;
            Vector3 spawnPos = Vector3.zero;

            for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
            {
                spawnPos = GetRandomPointAroundTarget(target.position, range);

                Collider2D hit = Physics2D.OverlapCircle(spawnPos, radius * 2f, safeZoneLayer);
                if (hit == null)
                {
                    success = true;
                    break;
                }
            }

            if (!success)
                continue;

            PoolObject spawned = ObjectPoolManager.Instance.Spawn(poolPrefab, spawnPos, Quaternion.identity);
            if (spawned == null) continue;

            spawned.gameObject.layer = safeLayer;

            SafeZone zone = spawned.GetComponent<SafeZone>();
            if (zone != null)
            {
                // 기존 장판 수치 초기화
                zone.InitializeSafe(radius, previewTime, activeTime);

                if (SafeZonePatternController.Instance != null)
                {
                    SafeZonePatternController.Instance.StartPattern(previewTime, activeTime, 10f, 0.5f);
                }
            }
        }
    }

    private Vector3 GetRandomPointAroundTarget(Vector3 center, float range)
    {
        Vector2 offset = Random.insideUnitCircle * range;
        return new Vector3(center.x + offset.x, center.y + offset.y, 0f);
    }

    private int GetLayerFromMask(LayerMask mask)
    {
        int bitmask = mask.value;
        if (bitmask == 0) return 0;

        for (int i = 0; i < 32; i++)
        {
            if ((bitmask & (1 << i)) != 0)
                return i;
        }

        return 0;
    }
}