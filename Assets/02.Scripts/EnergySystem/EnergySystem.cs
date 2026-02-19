using System;
using UnityEngine;
using UnityEngine.UI;

public class EnergySystem : MonoBehaviour
{
    [SerializeField] private int _maxEnergy = 5;
    [SerializeField] private float _increaseEnergyTime = 30;
    [SerializeField] private Text _energyCountText;
    [SerializeField] private Text _energyTimeText;

    public Action<int> OnUpdateUI;

    private int _currentEnergy;
    private float _timer;

    public int Timer => (int)_timer;


    private void OnEnable()
    {
        OnUpdateUI += UpdateTimerUI;
    }

    private void OnDisable()
    {
        OnUpdateUI -= UpdateTimerUI;
    }



    private void Update()
    {
        IncreaseTimer();

        OnUpdateUI?.Invoke(Timer);
    }

    private void IncreaseTimer()
    {
        if(_currentEnergy >= _maxEnergy)
            return;

        if(_timer >= _increaseEnergyTime)
        {
            _currentEnergy++;
            _timer = 0;
        }

        _timer += Time.deltaTime;
    }

    private void UpdateTimerUI(int time)
    {
        _energyCountText.text = _currentEnergy.ToString();

        if (_currentEnergy >= _maxEnergy)
        {
            _energyTimeText.text = string.Empty;
            return;
        }

        TimeSpan timeUI = TimeSpan.FromSeconds(time);
        string formatted = timeUI.ToString(@"mm\:ss");

        _energyTimeText.text = formatted;
    }

    //
    [ContextMenu("에너지 초기화")]
    public void Init()
    {
        _currentEnergy = 0;
    }

    [ContextMenu("에너지 상승")]
    public void IncreaseEnergy()
    {
        _currentEnergy++;
        _timer = 0;
    }
    //
}
