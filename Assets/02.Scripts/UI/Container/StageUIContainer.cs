using System;
using UnityEngine;
using UnityEngine.UI;

public class StageUIContainer : MonoBehaviour, IUIContainer
{
   
    [Header("가로 UI")]
    [SerializeField] private StageUIReference _horizontal;
    
    [Header("세로 UI")]
    [SerializeField] private StageUIReference _vertical;

    public GameObject Root => Current.Root;
    public StageUIReference Current => UIManager.Instance.IsVertical ? _vertical : _horizontal;
    public StageTimerTextUI TimerUI => Current.TimerUI;
    public GameObject GameOverPanel => Current.GameOverPanel;
    public GameObject StageClearPanel => Current.StageClearPanel;
    public GameObject BossHP => Current.BossHP;
    public StageWaveBannerUI WaveBannerUI => Current.WaveBannerUI;
    public RewardSystemUI ClearRewardUI => Current.ClearRewardUI;
    public Button SettingButton => Current.SettingButton;

    private void Start()
    {
        _horizontal.SettingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        _vertical.SettingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        
        OnOrientationChanged(UIManager.Instance.IsVertical);
        UIManager.Instance.OnOrientationChanged += OnOrientationChanged;
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnOrientationChanged -= OnOrientationChanged;
    }

    private void OnOrientationChanged(bool isVertical)
    {
        // UIManager 참조 갱신
        _horizontal.Root.SetActive(!isVertical);
        _vertical.Root.SetActive(isVertical);
        UIManager.Instance.UpdateStageUIReference(this);
        
    }
}

[Serializable]
public class StageUIReference
{
    public GameObject Root;
    public StageTimerTextUI TimerUI;
    public GameObject GameOverPanel;
    public GameObject StageClearPanel;
    public GameObject BossHP;
    public StageWaveBannerUI WaveBannerUI;
    public RewardSystemUI ClearRewardUI;
    public Button SettingButton;

}
