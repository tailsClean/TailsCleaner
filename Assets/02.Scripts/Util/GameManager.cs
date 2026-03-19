using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; private set => instance = value;}

    
    public EnergySystem _energySystem;

    public static int EnergyCount = 125;
    public const int SPEND_ENERGY = 1;
    public int _maxEnergy;

    public TowerTable _currentTower;

    public StageTable _currentStage;
    public int _currentStageId;
    public int _currentStageIndex;

    [SerializeField] private VoidEventChannelSO OnEnergyChange;
    private Dictionary<int, int> _maxClearStageIndexByTower = new Dictionary<int, int>();

    private const string CLEAR_STAGE_KEY_PREFIX = "TowerClear_";

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

        DontDestroyOnLoad(gameObject);
        _maxEnergy = _energySystem.MaxEnergy;
        // EnergyCount = _maxEnergy;

        LoadStageProgress();
    }

    public void EnterStage()
    {
        if (_energySystem.IsStartInGame)
        {
            UIManager.Instance.GoToStage();
        }
    }

    public void UpdateEnergyCount(int energy)
    {
        EnergyCount = energy;
        OnEnergyChange.OnStartEvent();
    }

    public void SetCurrentStage(StageTable stage)
    {
        _currentStage = stage;

        if (stage != null)
        {
            _currentStageId = stage.stage_id;
            _currentStageIndex = stage.stage_index;
        }
        else
        {
            _currentStageId = 0;
            _currentStageIndex = 0;
        }
    }

    public int GetMaxClearStageIndex(int towerId)
    {
        if (_maxClearStageIndexByTower.TryGetValue(towerId, out int clearedIndex))
            return clearedIndex;

        return 0;
    }

    public void MarkStageCleared(int towerId, int stageIndex)
    {
        int current = GetMaxClearStageIndex(towerId);

        if (stageIndex > current)
        {
            _maxClearStageIndexByTower[towerId] = stageIndex;
            SaveTowerProgress(towerId, stageIndex);

            Debug.Log($"[GameManager] Save Clear / towerId={towerId}, stageIndex={stageIndex}");
        }
    }

    private void SaveTowerProgress(int towerId, int stageIndex)
    {
        PlayerPrefs.SetInt(CLEAR_STAGE_KEY_PREFIX + towerId, stageIndex);
        PlayerPrefs.Save();
    }

    private void LoadStageProgress()
    {
        _maxClearStageIndexByTower.Clear();

        // 임시: 현재 타워 범위만 하드코딩
        for (int towerId = 5001; towerId <= 5007; towerId++)
        {
            int clearedIndex = PlayerPrefs.GetInt(CLEAR_STAGE_KEY_PREFIX + towerId, 0);

            if (clearedIndex > 0)
                _maxClearStageIndexByTower[towerId] = clearedIndex;

            Debug.Log($"[GameManager] Load Clear / towerId={towerId}, stageIndex={clearedIndex}");
        }
    }

    public void ClearStageProgress()
    {
        for (int towerId = 5001; towerId <= 5007; towerId++)
        {
            PlayerPrefs.DeleteKey(CLEAR_STAGE_KEY_PREFIX + towerId);
        }

        PlayerPrefs.Save();
        _maxClearStageIndexByTower.Clear();

        Debug.Log("[GameManager] Stage progress cleared.");
    }
}