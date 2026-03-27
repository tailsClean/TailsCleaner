using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;


public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private string nextSceneName = "GameScene";

    void OnEnable()
    {
        StartCoroutine(DownloadAllAddressables());
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

            if (progressText != null)
                progressText.text = $"{(int)(progress * 100)}%";

            yield return null;
        }

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            LoadNextScene();
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