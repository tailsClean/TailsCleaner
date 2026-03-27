using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;


public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _progressText;
    private const float _atLeastTime = 5;
    private float _currentTime = 0;

    void OnEnable()
    {
        StartCoroutine(DownloadAllAddressables());
        _currentTime = 0;
        
    }

    IEnumerator DownloadAllAddressables()
    {
        // 전체 다운로드 크기 확인
        long totalSize = 0;
        foreach (var locator in Addressables.ResourceLocators)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(locator.Keys);
            yield return sizeHandle;
            totalSize += sizeHandle.Result;
            Addressables.Release(sizeHandle);
        }

        if (totalSize == 0)
        {
            // 이미 모두 캐시됨
            LoadNextScene();
            yield break;
        }

        // 전체 다운로드 시작
        var downloadHandle = Addressables.DownloadDependenciesAsync(
            Addressables.ResourceLocators, 
            true
        );

        // 진행률 업데이트
        while (!downloadHandle.IsDone)
        {
            float progress = downloadHandle.GetDownloadStatus().Percent;

            if (_progressText != null)
                _progressText.text = $"{(int)(progress * 100)}%";
            _currentTime += Time.deltaTime;
            yield return null;
        }

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if(_currentTime >= _atLeastTime )
            {
                LoadNextScene();
            }
            else
            {
                _currentTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            Debug.LogError("다운로드 실패: " + downloadHandle.OperationException);
        }

        Addressables.Release(downloadHandle);
    }

    async void LoadNextScene()
    {
        await UIManager.Instance.LoadDataAndGoToLobby();
        gameObject.SetActive(false);
    }
}