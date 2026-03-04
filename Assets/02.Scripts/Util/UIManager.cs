using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.SceneManagement;
public interface UIContainer {}

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region  Scene 초기 설정
    //▼ UI 설정 오브젝트 
    [SerializeField] private GameObject _currentSceneUI;
    public Transform _stageTrans;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(_currentSceneUI != null)
        {
            Destroy(_currentSceneUI);
        }
        
        _currentSceneUI = OpenSceneUI($"{scene.name}");
    }

    private GameObject OpenSceneUI(string sceneName)
    {
       GameObject sceneUI = null;
       GameObject prefab = Resources.Load<GameObject>($"Prefabs/UI/{sceneName}UI");

       if(prefab != null)
       {
           sceneUI = Instantiate(prefab, transform);
           SetUpReference(sceneUI); //todo: 참조를 하고나서 저장해두는 방식으로 바꿔야할듯
       }
       return sceneUI;
    }
    private void SetUpReference(GameObject sceneUI)
    {
        if(sceneUI.TryGetComponent(out UIContainer container))
        {
            if(container is StageUIContainer stageUI) // UI 참조 연경
            {
                // StageUIContainer 참조로 들고 있기
                this._exitPanel = stageUI.ExitPanel;
                this._stageTimer = stageUI.TimerUI.GetComponent<StageTimerTextUI>();
                this._gameOverPanel = stageUI.GameOverPanel;
                this._stageClearPanel = stageUI.StageClearPanel;
            }
            if(container is TowerUIContainer towerUI)
            {
                // TowerUIContainer 참조로 들고 있기
                this._energyPanel = towerUI.EnergyPanel.GetComponent<EnergyPanel>();
            }
        }
    }

    #endregion
    [SerializeField]private VoidEventChannelSO _onStartInGame;

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
        _onStartInGame.OnStartEvent();
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

    #region ExitPanel
    [SerializeField] private GameObject _exitPanel;

    public void ChangeStateExitPanel()
    {
        if(_exitPanel != null)
        {
            _exitPanel.SetActive(!_exitPanel.activeSelf);
        }
    }

    #endregion

    #region StageClearPanel
    [SerializeField] private GameObject _stageClearPanel;
    public void OpenClear()
    {
        _stageClearPanel.SetActive(true);
    }
    
    #endregion

    #region GameOverPanel
    [SerializeField] private GameObject _gameOverPanel;
     public void OpenGameOver()
    {
        _gameOverPanel.SetActive(true);
    }
    
    #endregion
    
    #region StageTimer
    [SerializeField] private StageTimerTextUI _stageTimer;
    public StageTimerTextUI StageTimer => _stageTimer;

    #endregion

    #region EnergyUI
    [SerializeField] private EnergyPanel _energyPanel;
    public EnergyPanel EnergyPanel => _energyPanel;
    #endregion

}
