using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Unity.Cinemachine;


public class StageMapLoader : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;
    [SerializeField] private CinemachineConfiner2D _confiner;
    private AsyncOperationHandle<GameObject> _currentMapInstance;

    public async Task LoadMap(string mapResource)
    { 
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
            transform
        );  
        await _currentMapInstance.Task;
        GameObject mapInstance = _currentMapInstance.Result;   
      
        _confiner.BoundingShape2D = mapInstance.GetComponent<Collider2D>();
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