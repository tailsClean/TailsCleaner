using System;
using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private int _maxEnergy = 5;
    public int MaxEnergy => _maxEnergy;
    [SerializeField] private float _increaseEnergyTime = 30;
    // [SerializeField] private Text _energyCountText;
    // [SerializeField] private Text _energyTimeText;
    [SerializeField] private IntEventChannelSO _onIncreaseEnergy;
    [SerializeField] private IntEventChannelSO _onStartInGame;

    private int _currentEnergy;
    public int CurrentEnergy => _currentEnergy;
    private float _timer;

    public event Action<int> OnUpdateUI;

    public int Timer => (int)_timer;
    public bool IsStartInGame => _currentEnergy > 0;
    void Awake()
    {
        _currentEnergy = _maxEnergy;
    }
    private void OnEnable()
    {
        // OnUpdateUI += UpdateTimerUI;
        _onIncreaseEnergy.AddListener(IncreaseEnergy);
        _onStartInGame.AddListener(SpendEnergy);
    }

    private void OnDisable()
    {
        // OnUpdateUI -= UpdateTimerUI;
        _onIncreaseEnergy.RemoveListener(IncreaseEnergy);
        _onStartInGame.RemoveListener(SpendEnergy);
    }

    private void Update()
    {
        IncreaseTimer();

        OnUpdateUI?.Invoke(Timer);
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


    public void IncreaseEnergy(int count) => _currentEnergy += count;

    // 에너지 감소(사용) 메서드
    public void SpendEnergy(int count)
    {
        if(_currentEnergy <= 0)
        {
            _currentEnergy = 0;
            return;
        }    
        _currentEnergy -= count;
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
