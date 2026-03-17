using UnityEngine;

public class StageMapLoader : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;

    private GameObject _currentMapInstance;

    public void LoadMap(string mapResource)
    {
        if (string.IsNullOrWhiteSpace(mapResource))
        {
            Debug.LogWarning("[StageMapLoader] mapResource is null or empty.");
            return;
        }

        mapResource = mapResource.Trim();

        string path = $"Prefabs/Map/{mapResource}";
        Debug.Log($"[StageMapLoader] load path = '{path}'");

        GameObject mapPrefab = Resources.Load<GameObject>(path);
        if (mapPrefab == null)
        {
            Debug.LogError($"[StageMapLoader] Map prefab not found: {path}");
            return;
        }

        Instantiate(mapPrefab, transform.position, Quaternion.identity, transform);
    }

    public void ClearMap()
    {
        if (_currentMapInstance != null)
        {
            Destroy(_currentMapInstance);
            _currentMapInstance = null;
        }
    }
}