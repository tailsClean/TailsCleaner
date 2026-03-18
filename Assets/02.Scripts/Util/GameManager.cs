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
        _maxEnergy = _energySystem.MaxEnergy;
        //EnergyCount = _maxEnergy;
    }

    public void EnterStage()
    {
        if(_energySystem.IsStartInGame)
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
        
        if(stage != null)
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
        {
            return clearedIndex;
        }
        return 0; // 기본값은 0
    }

    public bool IsStageUnlocked(int towerId, int stageIndex)
    {
        // 모든 타워의 1스테이지는 항상 열림
        if (stageIndex == 1)
            return true;

        int maxCleared = GetMaxClearStageIndex(towerId);
        return stageIndex <= maxCleared + 1;
    }

    public void MarkStageCleared(int towerId, int stageIndex)
    {
        int current = GetMaxClearStageIndex(towerId);
        if (stageIndex > current)
        {
            _maxClearStageIndexByTower[towerId] = stageIndex;
        }
    }   
}
