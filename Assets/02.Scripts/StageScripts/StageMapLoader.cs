using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Unity.Cinemachine;
using System.Collections;


public class StageMapLoader : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;
    [SerializeField] private CinemachineConfiner2D _confiner;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private Transform player;
    [SerializeField] private RuleBasedMonsterSpawner _spawner;

    private AsyncOperationHandle<GameObject> _currentMapInstance;

    public async Task LoadMap(string mapResource)
    {
        _confiner.enabled = false;

        if (string.IsNullOrWhiteSpace(mapResource))
        {
            Debug.LogWarning("[StageMapLoader] mapResource is null or empty.");
            return;
        }

        mapResource = mapResource.Trim();
        Debug.Log($"[StageMapLoader] load address = '{mapResource}'");

        ClearMap();

        _currentMapInstance = Addressables.InstantiateAsync(
            mapResource,
            transform.position,
            Quaternion.identity,
            _mapRoot != null ? _mapRoot : transform
        );

        try
        {
            await _currentMapInstance.Task;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageMapLoader] Addressables load failed. key={mapResource}, error={ex.Message}");
            return;
        }

        if (!_currentMapInstance.IsValid() || _currentMapInstance.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"[StageMapLoader] map instantiate failed. key={mapResource}");
            return;
        }

        GameObject mapInstance = _currentMapInstance.Result;
        if (mapInstance == null)
        {
            Debug.LogError($"[StageMapLoader] map instance is null. key={mapResource}");
            return;
        }

        // 카메라 confiner용 collider
        Collider2D mapCollider = mapInstance.GetComponent<Collider2D>();
        if (mapCollider != null)
        {
            _confiner.BoundingShape2D = mapCollider;
        }
        else
        {
            Debug.LogWarning($"[StageMapLoader] Root Collider2D missing on map prefab. key={mapResource}");
        }

        // 플레이어 추적 세팅
        var target = cinemachineCamera.Target;
        target.TrackingTarget = player;
        cinemachineCamera.Target = target;

        _confiner.enabled = true;
        _confiner.InvalidateBoundingShapeCache();

        // 스폰 bounds 전달
        BindSpawnBoundsFromMap(mapInstance);
    }

    private void BindSpawnBoundsFromMap(GameObject mapInstance)
    {
        if (_spawner == null)
        {
            _spawner = FindFirstObjectByType<RuleBasedMonsterSpawner>();
        }

        if (_spawner == null)
        {
            Debug.LogWarning("[StageMapLoader] RuleBasedMonsterSpawner not found.");
            return;
        }

        // 1순위: wall 컴포넌트를 찾고 그 오브젝트의 Collider2D 사용
        wall wallComponent = mapInstance.GetComponentInChildren<wall>(true);
        if (wallComponent != null)
        {
            Collider2D wallCollider = wallComponent.GetComponent<Collider2D>();
            if (wallCollider != null)
            {
                _spawner.SetSpawnBounds(wallCollider.bounds);
                Debug.Log($"[StageMapLoader] Spawn bounds bound from wall collider: {wallCollider.name}");
                return;
            }
        }

        // 2순위: 이름이 WallCol인 Collider2D 검색
        Collider2D[] allColliders = mapInstance.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < allColliders.Length; i++)
        {
            if (allColliders[i] != null && allColliders[i].name == "WallCol")
            {
                _spawner.SetSpawnBounds(allColliders[i].bounds);
                Debug.Log($"[StageMapLoader] Spawn bounds bound from collider name: {allColliders[i].name}");
                return;
            }
        }

        // 3순위: 루트 맵 collider fallback
        Collider2D rootCollider = mapInstance.GetComponent<Collider2D>();
        if (rootCollider != null)
        {
            _spawner.SetSpawnBounds(rootCollider.bounds);
            Debug.Log("[StageMapLoader] Spawn bounds fallback to root collider.");
            return;
        }

        Debug.LogWarning("[StageMapLoader] No spawn bounds collider found in loaded map.");
    }

    public void ClearMap()
    {
        if (_currentMapInstance.IsValid())
        {
            Addressables.ReleaseInstance(_currentMapInstance);
        }
    }

    private void OnDestroy()
    {
        ClearMap();
    }
}