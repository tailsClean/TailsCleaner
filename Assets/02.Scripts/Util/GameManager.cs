using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance { get => instance; private set => instance = value;}

    
    public EnergySystem _energySystem;
    public int EnergyCount;
    public const int SPEND_ENERGY = 1;
    public int _maxEnergy;

    public TowerTable _currentTower;

    public StageTable _currentStage;
    public int _currentStageId;
    public int _currentStageIndex;

    //파이어베이스
    private DatabaseReference db;
    public DatabaseReference DB => db;
    public string UID => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
    
    [SerializeField] private VoidEventChannelSO OnEnergyChange;
    private Dictionary<int, int> _maxClearStageIndexByTower = new Dictionary<int, int>();

    private async void Awake()
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
        db = FirebaseDatabase.DefaultInstance.RootReference;
    }
    
    private void Start()
    {
        FirebaseManager.Instance.AddLoadData(LoadStageProgress);
        FirebaseManager.Instance.AddSaveData(SaveEnergyCount);
    }
    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RemoveLoadData(LoadStageProgress);
            FirebaseManager.Instance.RemoveSaveData(SaveEnergyCount);
        }
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

    public async Task SaveEnergyCount()
    {
         await db.Child("users").Child(UID)
                .Child("System").Child("Energy")
                .SetValueAsync(EnergyCount);
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

    public async Task MarkStageCleared(int towerId, int stageIndex)
    {
        int current = GetMaxClearStageIndex(towerId);

        if (stageIndex > current)
        {
            _maxClearStageIndexByTower[towerId] = stageIndex;
            await SaveTowerProgress(towerId, stageIndex);

            Debug.Log($"[GameManager] Save Clear / towerId={towerId}, stageIndex={stageIndex}");
        }
    }

    public async Task SaveTowerProgress(int towerId, int stageIndex)
    {
        if(UID != null)
        {
            await db.Child("users").Child(UID)
                    .Child("dungeon")
                    .Child(towerId.ToString())
                    .SetValueAsync(stageIndex);
        }
    }
    
    public async Task LoadStageProgress()
    {
        _maxClearStageIndexByTower.Clear();

        var snapshot = await db.Child("users").Child(UID)
                               .Child("dungeon").GetValueAsync();

        if(!snapshot.Exists)
        {
            Debug.Log("snapshot don`t exists");
            return;
        }
        
        for (int towerId = 5001; towerId <= 5007; towerId++)
        {
            var child = snapshot.Child(towerId.ToString());
            if(child.Exists)
            {
                int clearedIndex = int.Parse(child.Value.ToString());
            
                if (clearedIndex > 0)
                    _maxClearStageIndexByTower[towerId] = clearedIndex;

            }
        }
        
    }

    public async Task ClearStageProgress()
    {
        await db.Child("users").Child(UID).Child("dungeon").RemoveValueAsync();

        _maxClearStageIndexByTower.Clear();

        Debug.Log("[GameManager] Stage progress cleared.");
    }

}