using System;
using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private int _maxEnergy = 5;
    public int MaxEnergy => _maxEnergy;
    [SerializeField] private float _increaseEnergyTime = 30;
    [SerializeField] private IntEventChannelSO _onIncreaseEnergy;
    [SerializeField] private VoidEventChannelSO _onStartStage;

    private int _currentEnergy;
    public int CurrentEnergy => _currentEnergy;
    private float _timer;

    public int Timer => (int)_timer;
    public bool IsStartInGame => _currentEnergy > 0;
    void Awake()
    {
        _currentEnergy = 125;
    }
    private void OnEnable()
    {
        // OnUpdateUI += UpdateTimerUI;
        _onIncreaseEnergy.AddListener(IncreaseEnergy);
        _onStartStage.AddPriorityListener(SpendEnergy, 0);
    }

    private void OnDisable()
    {
        // OnUpdateUI -= UpdateTimerUI;
        _onIncreaseEnergy.RemoveListener(IncreaseEnergy);
        _onStartStage.RemovePriorityListener(SpendEnergy, 0);
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
    public void SpendEnergy()
    {
        if(_currentEnergy <= 0)
        {
            _currentEnergy = 0;
            return;
        }    
        _currentEnergy -= GameManager.SPEND_ENERGY;
        GameManager.Instance.UpdateEnergyCount(_currentEnergy);

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
    
}
