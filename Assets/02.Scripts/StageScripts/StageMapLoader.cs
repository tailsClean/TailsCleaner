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
            transform
        );  
        await _currentMapInstance.Task;
        await Task.Delay(1000);
        

        GameObject mapInstance = _currentMapInstance.Result;   
        
        var col = mapInstance.GetComponent<Collider2D>();
        _confiner.BoundingShape2D = col;

        var target = cinemachineCamera.Target;
        target.TrackingTarget = player;
        cinemachineCamera.Target = target;

        _confiner.enabled = true;
         _confiner.InvalidateBoundingShapeCache();
    

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