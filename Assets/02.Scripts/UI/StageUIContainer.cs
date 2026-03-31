using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageUIContainer : MonoBehaviour, IUIContainer
{
    [SerializeField] private Button _settingButton;
    [SerializeField] private GameObject _exitPanel;
    public GameObject ExitPanel => _exitPanel;

    [Header("가로 UI")]
    [SerializeField] private StageUIReference _horizontal;

    [Header("세로 UI")]
    [SerializeField] private StageUIReference _vertical;

    public StageUIReference Current => UIManager.Instance.IsVertical ? _vertical : _horizontal;

    public StageTimerTextUI TimerUI => Current.TimerUI;
    public GameObject GameOverPanel => Current.GameOverPanel;
    public GameObject StageClearPanel => Current.StageClearPanel;
    public GameObject BossHP => Current.BossHP;
    public StageWaveBannerUI WaveBannerUI => Current.WaveBannerUI;
    public RewardSystemUI ClearRewardUI => Current.ClearRewardUI;

    [SerializeField] private List<UIGroup> _uiGroupList;
    public Dictionary<UI_GROUP, UIGroup> _uiDict;

    private void Awake()
    {
        _uiDict = new Dictionary<UI_GROUP, UIGroup>();
        foreach (var uiGroup in _uiGroupList)
            _uiDict.Add(uiGroup.Group, uiGroup);
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
        UIManager.Instance.UpdateStageUIReference(this);
    }
    private void Start()
    {
        _settingButton.onClick.AddListener(() => {
            UIManager.Instance.ChangeStateSettingPanel();
        });
    }

    public void SetActiveUiGroup(UI_GROUP uiState, bool active)
    {
        if (_uiDict.TryGetValue(uiState, out var uIGroup))
            uIGroup.gameObject.SetActive(active);
        else
            Debug.LogWarning(uiState + "에 해당하는 UI그룹이 없습니다.");
    }
}

[Serializable]
public class StageUIReference
{
    public StageTimerTextUI TimerUI;
    public GameObject GameOverPanel;
    public GameObject StageClearPanel;
    public GameObject BossHP;
    public StageWaveBannerUI WaveBannerUI;
    public RewardSystemUI ClearRewardUI;
}
