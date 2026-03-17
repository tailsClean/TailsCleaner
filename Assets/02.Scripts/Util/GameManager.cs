using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; private set => instance = value;}


    public EnergySystem _energySystem;

    public static int EnergyCount;
    public const int SPEND_ENERGY = 1;
    public int _maxEnergy;
    public TowerTable _currentTower;

    public StageTable _currentStage;
    public int _currentStageId;
    public int _currentStageIndex;

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
        EnergyCount = _maxEnergy;
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
        UIManager.Instance.EnergyPanel?.UpdateEnergyText();
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
        if (stageIndex <= 1)
        { return true; } // 1스테이지는 잠금 해제    

        int maxClearedIndex = GetMaxClearStageIndex(towerId);
        return stageIndex <= maxClearedIndex + 1; // 다음 스테이지까지 허용
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
