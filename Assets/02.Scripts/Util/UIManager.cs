using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;


public interface IUIContainer { }
public interface IOrientationHandler
{
    void OnOrientationChanged(bool isVertical);
}

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance { get => instance; private set => instance = value; }
    
    public event Action<bool> OnOrientationChanged; // 가로/세로 변경 이벤트
    private bool _isVertical;
    public bool IsVertical
    {
        get => _isVertical;
        set
        {
            if( _isVertical == value ) return; // 값이 변경되지 않았으면 이벤트 발생 안함
            _isVertical = value;
            OnOrientationChanged?.Invoke(_isVertical); // 이벤트 발생
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
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
    [SerializeField] private GameObject _currentSceneUI;
    public Transform _stageTrans;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_currentSceneUI != null)
        {
            Destroy(_currentSceneUI);
        }

        if (_settingPanel != null)
        {
            Destroy(_settingPanel);
        }

        _currentSceneUI = OpenSceneUI($"{scene.name}");
    }

    private GameObject OpenSceneUI(string sceneName)
    {
        GameObject sceneUI = null;
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/UI/{sceneName}UI");

        if (prefab != null)
        {
            sceneUI = Instantiate(prefab, transform);
            sceneUI.transform.SetAsFirstSibling();
            SetUpReference(sceneUI);
        }
        else
        {
            Debug.Log("없음");
        }

        return sceneUI;
    }

    private void SetUpReference(GameObject sceneUI)
    {
        if (sceneUI.TryGetComponent(out IUIContainer container))
        {
            if (container is StageUIContainer stageUI)
            {
                UpdateStageUIReference(stageUI);
                OnOrientationChanged += _=> UpdateStageUIReference(stageUI);
            }

            if (container is LobbyUIContainer lobbyUI)
            {
                UpdateLobbyUIReference(lobbyUI);
                OnOrientationChanged += _ => UpdateLobbyUIReference(lobbyUI);
            }
        }
    }
    #endregion

    [SerializeField] private VoidEventChannelSO _onStartInGame;

    public void UpdateStageUIReference(StageUIContainer stageUI)
    {
        var reference = stageUI.Current;
        this._stageTimer = reference.TimerUI;
        this._gameOverPanel = reference.GameOverPanel;
        this._stageClearPanel = reference.StageClearPanel;
        this._BossHP = reference.BossHP;
        this._stageWaveBanner = reference.WaveBannerUI;
        this._clearRewardUI = reference.ClearRewardUI;
    }

    public void UpdateLobbyUIReference(LobbyUIContainer lobbyUI)
    {
        var reference = lobbyUI.Current;
        this._dungeonSelect = lobbyUI.DungeonSelect;
        this._stageSelect = lobbyUI.StageSelect;
    }

    public async Task LoadDataAndGoToLobby()
    {
        await GameManager.Instance.LoadStageProgress();
        SceneManager.LoadScene("LobbyScene");

        await FirebaseManager.Instance.Load();
    }

    public void GoToLobby() 
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void GoToStage()
    {
        SceneManager.sceneLoaded -= OnStageLoaded; // [추가] 중복 등록 방지
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
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #region Orientation
    [SerializeField] private ItemUI _HorizontalItem;
    [SerializeField] private ItemUI _VerticalItem;
    private ItemUI _currentItemUI;

    [SerializeField] private CanvasScaler _canvasScaler;
    private void Start()
    {
        // 초기 화면 방향 설정
        bool isVertical = PlayerPrefs.GetInt("IsVertical", 0) == 1;
        SetOrientation(isVertical); 
    }

    public void SetOrientation(bool isVertical)
    {
        Screen.orientation = isVertical
            ? ScreenOrientation.Portrait
            : ScreenOrientation.LandscapeLeft;

        if (_canvasScaler != null)
        {
            _canvasScaler.referenceResolution = isVertical
                ? new Vector2(1080, 1920)
                : new Vector2(1920, 1080);
        }
        _currentItemUI = isVertical ? _VerticalItem : _HorizontalItem;

        IsVertical = isVertical;
    }

    #endregion

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
        if (_dungeonSelect != null)
        {
            _dungeonSelect.SetActive(!_dungeonSelect.activeSelf);
        }
    }

    public void ChangeStateStageSelect()
    {
        if (_stageSelect != null)
        {
            _stageSelect.SetActive(!_stageSelect.activeSelf);
        }
    }
    #endregion

    #region BossUI
    private GameObject _BossHP;

    public void ChangeStateBossHP()
    {
        if (_BossHP != null)
        {
            _BossHP.SetActive(!_BossHP.activeSelf);
        }
    }
    #endregion

    #region ItemManager
    [SerializeField] public GameObject _inventoryUI;
    [SerializeField] public GameObject _relicUI;
    [SerializeField] public GameObject _equipUI;
    [SerializeField] public GameObject _myStatsUI;

    public void ChangeStateInventory()
    {
        if (_inventoryUI != null)
        {
            _currentItemUI._inventoryUI.SetActive(!_inventoryUI.activeSelf);
        }
    }

    public void ChangeStateRelic()
    {
        if (_relicUI != null)
        {
            _currentItemUI._playerRelicUI.SetActive(!_relicUI.activeSelf);
        }
    }

    public void ChangeStateEquipUI()
    {
        if (_equipUI != null)
        {
            _currentItemUI._playerEquipUI.SetActive(!_equipUI.activeSelf);
        }
    }
    public void ChangeStateMyStatsUI()
    {
        if (_myStatsUI != null)
        {
            _myStatsUI.SetActive(!_myStatsUI.activeSelf);
        }
    }
    #endregion

    #region StageWaveBanner
    private StageWaveBannerUI _stageWaveBanner;
    public StageWaveBannerUI StageWaveBanner => _stageWaveBanner;
    #endregion

    #region RewardUI
    // [추가] 결과 보상 UI 외부 접근용
    private RewardSystemUI _clearRewardUI;
    public RewardSystemUI ClearRewardUI => _clearRewardUI;

    private RewardSystemUI _failRewardUI;
    public RewardSystemUI FailRewardUI => _failRewardUI;
    #endregion

    #region ImpossiblePanel
    private ImpossibelPannel _impossiblePanel;
    public ImpossibelPannel ImpossiblePanel => _impossiblePanel;
    [SerializeField] private GameObject _impossiblePrefab;

    public void ChangeStateImpossiblePanel()
    {
        if (_impossiblePanel == null)
        {
            if (_impossiblePrefab == null)
            {
                Debug.LogError("_impossiblePrefab이 할당되지 않았습니다!");
                return;
            }

            _impossiblePanel = Instantiate(_impossiblePrefab, this.transform).GetComponent<ImpossibelPannel>();
            _impossiblePanel.transform.SetAsLastSibling();
            _impossiblePanel.gameObject.SetActive(true);
        }
        else
        {
            _impossiblePanel.gameObject.SetActive(!_impossiblePanel.gameObject.activeSelf);
        }
    }
    #endregion

    #region ConfirmPanel
    private ConfirmPannel _confirmPanel;
    public ConfirmPannel ConfirmPanel => _confirmPanel;
    [SerializeField] private GameObject _confirmPrefab;

    public void ChangeStateConfirmPanel()
    {
        if (_confirmPanel == null)
        {
            if (_confirmPrefab == null)
            {
                Debug.LogError("_confirmPrefab 이 할당되지 않았습니다!");
                return;
            }
            _confirmPanel = Instantiate(_confirmPrefab, this.transform).GetComponent<ConfirmPannel>();
            _confirmPanel.transform.SetAsLastSibling();
            _confirmPanel.gameObject.SetActive(true);
        }
        else
        {
            _confirmPanel.gameObject.SetActive(!_confirmPanel.gameObject.activeSelf);
        }
    }
    #endregion    
}