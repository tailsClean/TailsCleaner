using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance { get => instance; private set => instance = value;}
    
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public Action OnUIInitialized;

    private void Start()
    {
       SceneManager.sceneLoaded += OnSceneLoaded;
    }
    #region  Scene 초기 설정
    //▼ UI 설정 오브젝트 
    [SerializeField] private GameObject currentSceneUI;
    public Transform stageTrans;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(currentSceneUI != null)
        {
            Destroy(currentSceneUI);
        }
        
        currentSceneUI = OpenSceneUI($"{scene.name}");
    }

    private GameObject OpenSceneUI(string sceneName)
    {
       GameObject sceneUI = null;
       GameObject prefab = Resources.Load<GameObject>($"Prefabs/UI/{sceneName}UI");

       Debug.Log($"[UIManager] 로드 시도: Prefabs/UI/{sceneName}UI → {(prefab == null ? "NULL ❌" : "성공 ✅")}");

       if(prefab != null)
       {
           sceneUI = Instantiate(prefab, transform);

            if(sceneName.Equals("StageScene"))
            {
                
            }
       }
       return sceneUI;
    }

    #endregion


    public void GoToTower()
    {
        SceneManager.LoadScene("TowerScene");
    }

    public void GoToLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void GoToStage()
    {
        SceneManager.LoadScene("StageScene");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;// 에디터에서 실행 중인 게임을 종료
#else
        Application.Quit(); // 빌드된 게임을 종료
#endif    
    }
}
