using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
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
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region Scene 초기 설정
    //▼ UI 설정 오브젝트 
    [SerializeField] private GameObject _currentSceneUI;
    public Transform _stageTrans;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(_currentSceneUI != null)
        {
            Destroy(_currentSceneUI);
        }
        if(_settingPanel != null)
        {
            Destroy(_settingPanel);
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
           sceneUI.transform.SetAsFirstSibling();
           SetUpReference(sceneUI); //todo: 참조를 하고나서 저장해두는 방식으로 바꿔야할듯
       }
       else
        {
             Debug.Log("없음");
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
                this._stageTimer = stageUI.TimerUI.GetComponent<StageTimerTextUI>();
                this._gameOverPanel = stageUI.GameOverPanel;
                this._stageClearPanel = stageUI.StageClearPanel;
                this._BossHP = stageUI.BossHP;
                this._stageWaveBanner = stageUI.WaveBannerUI;
            }
            if (container is LobbyUIContainer lobbyUI)
            {
                // LobbyUIContainer 참조로 들고 있기
                this._dungeonSelect = lobbyUI.DungeonSelect;
                this._stageSelect = lobbyUI.StageSelect;
            }
        }
        
    }

    #endregion
    [SerializeField]private VoidEventChannelSO _onStartInGame;

    public async Task LoadDataAndGoToLobby()
    {
        await GameManager.Instance.LoadStageProgress();
        SceneManager.LoadScene("LobbyScene");
    }
    public void GoToLobby()
    {
        SceneManager.LoadScene("LobbyScene");    
    }
    

    public void GoToStage()
    {
        SceneManager.sceneLoaded += OnStageLoaded;
        SceneManager.LoadScene("StageScene");
    }

    private void OnStageLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "StageScene") return;

        SceneManager.sceneLoaded -= OnStageLoaded;

        Debug.Log("StartEvent 받음");
        _onStartInGame.OnStartEvent();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;// 에디터에서 실행 중인 게임을 종료
#else
        Application.Quit(); // 빌드된 게임을 종료
#endif    
    }

    #region SettingPanel
    private GameObject _settingPanel;
    [SerializeField] private GameObject _settingPrefab;
    public void ChangeStateSettingPanel()
    {
        if (_settingPanel == null)
        {
            if (_settingPrefab == null)
            {
                Debug.LogError("_settingPrefab이 할당되지 않았습니다!");
                return;
            }
            _settingPanel = Instantiate(_settingPrefab, this.transform);
            _settingPanel.transform.SetAsLastSibling();
            _settingPanel.SetActive(true);   
        }
        else
        {
            _settingPanel.SetActive(!_settingPanel.activeSelf);
        }
    }

    #endregion

    #region StageClearPanel
     private GameObject _stageClearPanel;
    public void OpenClear()
    {
        _stageClearPanel.SetActive(true);
        StageTimer.gameObject.SetActive(false);
    }
    
    #endregion

    #region GameOverPanel
    private GameObject _gameOverPanel;
     public void OpenGameOver()
    {
        _gameOverPanel.SetActive(true);
        StageTimer.gameObject.SetActive(false);
    }
    
    #endregion
    
    #region StageTimer
     private StageTimerTextUI _stageTimer;
    public StageTimerTextUI StageTimer => _stageTimer;

    #endregion

    #region EnergyUI
    private EnergyPanel _energyPanel;
    public EnergyPanel EnergyPanel => _energyPanel;

    public void SetEnergyPanel(EnergyPanel energyPanel)
    {
        _energyPanel = energyPanel;
    }

    #endregion

    #region LobbyUI
    private GameObject _dungeonSelect;
    private GameObject _stageSelect;
    public void ChangeStateDungeonSelect()
    {
        if(_dungeonSelect != null)
        {
            _dungeonSelect.SetActive(!_dungeonSelect.activeSelf);
        }
    }
    public void ChangeStateStageSelect()
    {
        if(_stageSelect != null)
        {
            _stageSelect.SetActive(!_stageSelect.activeSelf);
        }
    }
    #endregion

    #region BossUI
    private GameObject _BossHP;
    public void ChangeStateBossHP()
    {
        if(_BossHP != null)
        {
            _BossHP.SetActive(!_BossHP.activeSelf);
        }
    }
    #endregion

    #region ItemManager
    [SerializeField] public GameObject _inventoryUI;
    [SerializeField] public GameObject _relicUI;
    [SerializeField] public GameObject _equipUI;

     public void ChangeStateInventory()
    {
        if(_inventoryUI != null)
        {
            _inventoryUI.SetActive(!_inventoryUI.activeSelf);
        }
    }

     public void ChangeStateRelic()
    {
        if(_relicUI != null)
        {
            _relicUI.SetActive(!_relicUI.activeSelf);
        }
    }
     public void ChangeStateEquipUI()
    {
        if(_equipUI != null)
        {
            _equipUI.SetActive(!_equipUI.activeSelf);
        }
    }


    #endregion

    #region StageWaveBanner
    private StageWaveBannerUI _stageWaveBanner;
    public StageWaveBannerUI StageWaveBanner => _stageWaveBanner;
    #endregion

}
