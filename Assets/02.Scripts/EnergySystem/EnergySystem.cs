using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


public class EnergySystem : MonoBehaviour
{
    public static EnergySystem Instance;

    [SerializeField] private int _maxEnergy = 999;
    public int MaxEnergy => _maxEnergy;
    [SerializeField] private float _increaseEnergyTime = 30;
    [SerializeField] private IntEventChannelSO _onIncreaseEnergy;
    [SerializeField] private VoidEventChannelSO _onStartStage;

    private int _currentEnergy;
    public int CurrentEnergy => _currentEnergy;
    private const int _defaultEnergy = 125;
    private float _timer;

    public int Timer => (int)_timer;
    public bool IsStartInGame => _currentEnergy > 0;
    private async void Start()
    {
        FirebaseManager.Instance.AddLoadData(LoadEnergy);
        FirebaseManager.Instance.AddSaveData(SaveEnergyCancelTime);
    }
    private void OnEnable()
    {
        Debug.Log($"[EnergySystem] OnEnable instance={GetInstanceID()}");
        _onIncreaseEnergy.AddListener(IncreaseEnergy);
        _onStartStage.AddPriorityListener(SpendEnergy, 0);
    }

    private void OnDisable()
    {
        Debug.Log($"[EnergySystem] OnDisable instance={GetInstanceID()}");
        _onIncreaseEnergy.RemoveListener(IncreaseEnergy);
        _onStartStage.RemovePriorityListener(SpendEnergy, 0);
    }
    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RemoveLoadData(LoadEnergy);
            FirebaseManager.Instance.RemoveSaveData(SaveEnergyCancelTime);
        }
    }

    private void Update()
    {
         IncreaseTimer();
    }

    // 타이머 증가 메서드
    private void IncreaseTimer()
    {
        if(_currentEnergy >= _maxEnergy)
        {
            _timer = 0;
            return;
        }
        

        if(_timer >= _increaseEnergyTime)
        {
            _currentEnergy++;
            GameManager.Instance.UpdateEnergyCount(_currentEnergy);
            _timer = 0;
        }

        _timer += Time.deltaTime;
    }


    // private void UpdateTimerUI(int time)
    // {
    //     _energyCountText.text = _currentEnergy.ToString();

    //     if (_currentEnergy >= _maxEnergy)
    //     {
    //         _energyTimeText.text = string.Empty;
    //         return;
    //     }

    //     TimeSpan timeUI = TimeSpan.FromSeconds(time);
    //     string formatted = timeUI.ToString(@"mm\:ss");

    //     _energyTimeText.text = formatted;
    // }


    public void IncreaseEnergy(int count) 
    {
        _currentEnergy += count;
        GameManager.Instance.UpdateEnergyCount(_currentEnergy);
    }

    // 에너지 감소(사용) 메서드
    public bool CanSpendEnergy()
    {
        return _currentEnergy >= GameManager.SPEND_ENERGY;
    }

    public void SpendEnergy()
    {
        Debug.Log($"[EnergySystem] SpendEnergy called. instance={GetInstanceID()}, before={_currentEnergy}");

        if (!CanSpendEnergy())
        {
            _currentEnergy = Mathf.Max(0, _currentEnergy);
            GameManager.Instance.UpdateEnergyCount(_currentEnergy);
            Debug.Log("[EnergySystem] Not enough energy to start stage.");
            return;
        }

        _currentEnergy -= GameManager.SPEND_ENERGY;
        GameManager.Instance.UpdateEnergyCount(_currentEnergy);

        Debug.Log($"[EnergySystem] SpendEnergy finished. after={_currentEnergy}");
    }

    [ContextMenu("에너지 초기화")]
    public void Init()
    {
        _currentEnergy = 0;
    }

    [ContextMenu("에너지 상승")]
    public void AddEnergyTest()
    {
        _currentEnergy++;
        _timer = 0;
    }
    
    public async Task LoadEnergy()
    {
        var snapshot = await GameManager.Instance.DB.Child("users").Child(GameManager.Instance.UID)
                            .Child("System").GetValueAsync();

        if(!snapshot.Exists)
        {
            Debug.Log("snapshot don`t exists");
            _currentEnergy = _defaultEnergy;
            GameManager.Instance.UpdateEnergyCount(_currentEnergy);
            return;
        }
        var energyChild = snapshot.Child("Energy");
        
        if(energyChild.Exists)
        {
            _currentEnergy = int.Parse(energyChild.Value.ToString());
            
        }
        else
        {
            _currentEnergy = _defaultEnergy;
        }

        var timeChild = snapshot.Child("CancelTime");

        if (timeChild.Exists)
        {
            
            long cancelTime = EnergyCancelTimeCalculate(timeChild.Value.ToString());
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = now - cancelTime;

            int recharged = (int)(elapsedSeconds / (double)_increaseEnergyTime);
            _currentEnergy = Mathf.Min(_currentEnergy + recharged, _maxEnergy);

            _timer += (int)(elapsedSeconds % (double)_increaseEnergyTime);
             

        }
        Debug.Log($"{_currentEnergy} 현재 에너지");

        GameManager.Instance.UpdateEnergyCount(_currentEnergy);
    }

    private long EnergyCancelTimeCalculate(string value)
    {
        if (long.TryParse(value, out long cancelTime))
            return cancelTime;
        
        if (DateTimeOffset.TryParse(value, out DateTimeOffset dt))
            return dt.ToUnixTimeSeconds();
        
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
    private async Task SaveEnergyCancelTime() 
    {
         long cancelTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            await GameManager.Instance.DB.Child("users")
                                         .Child(GameManager.Instance.UID)
                                         .Child("System")
                                         .Child("CancelTime")
                                         .SetValueAsync(cancelTime); 
    }
}
    

