using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StageSelect : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _btnPrevStage;
    [SerializeField] private Button _btnNextStage;
    [SerializeField] private Button _btnStageEnter;
    [SerializeField] private Button _btnMainEnter;
    [SerializeField] private Button _btnSetting;

    [Header("패널")]
    [SerializeField] private GameObject _unlockedStagePanel;
    [SerializeField] private GameObject _lockedStagePanel;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI _txtStage;
    [SerializeField] private TextMeshProUGUI _txtLockedStage;
    [SerializeField] private TextMeshProUGUI _txtNeedStageReward;

    [Header("보상 이미지")]
    [SerializeField] private Image _imgRewardUnlocked;
    [SerializeField] private Image _imgRewardLocked;

    private StageTableSO _stageData;
    private List<StageTable> _towerStages = new List<StageTable>();
    private int _currentIndex = 0;

    private int CurrentTowerId
    {
        get
        {
            if (GameManager.Instance == null || GameManager.Instance._currentTower == null)
                return 0;

            return GameManager.Instance._currentTower.tower_id;
        }
    }

    private int StageCount => _towerStages.Count;

    private void Start()
    {
        if (_btnPrevStage != null) _btnPrevStage.onClick.AddListener(OnPrevStage);
        if (_btnNextStage != null) _btnNextStage.onClick.AddListener(OnNextStage);
        if (_btnStageEnter != null) _btnStageEnter.onClick.AddListener(OnEnterStage);
        if (_btnMainEnter != null) _btnMainEnter.onClick.AddListener(OnBackToDungeonSelect);
        if (_btnSetting != null) _btnSetting.onClick.AddListener(OnOpenSetting);

        _stageData = DataManager.Instance.GetSOData<StageTableSO>();

        BuildStageList();
        Refresh(false);
    }

    private void OnDestroy()
    {
        if (_btnPrevStage != null) _btnPrevStage.onClick.RemoveListener(OnPrevStage);
        if (_btnNextStage != null) _btnNextStage.onClick.RemoveListener(OnNextStage);
        if (_btnStageEnter != null) _btnStageEnter.onClick.RemoveListener(OnEnterStage);
        if (_btnMainEnter != null) _btnMainEnter.onClick.RemoveListener(OnBackToDungeonSelect);
        if (_btnSetting != null) _btnSetting.onClick.RemoveListener(OnOpenSetting);
    }

    private void BuildStageList()
    {
        _towerStages.Clear();

        if (_stageData == null)
        {
            Debug.LogError("[StageSelect] StageTableSO is null.");
            return;
        }

        int towerId = CurrentTowerId;
        if (towerId <= 0)
        {
            Debug.LogWarning("[StageSelect] current tower is invalid.");
            return;
        }

        for (int i = 0; i < _stageData.dataList.Count; i++)
        {
            StageTable stage = _stageData.dataList[i];
            if (stage.tower_id == towerId)
            {
                _towerStages.Add(stage);
            }
        }

        _towerStages.Sort((a, b) => a.stage_index.CompareTo(b.stage_index));

        if (_towerStages.Count == 0)
        {
            Debug.LogWarning($"[StageSelect] No stage found for towerId={towerId}");
            return;
        }

        _currentIndex = 0;
    }

    private void OnPrevStage()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        Refresh(true);
    }

    private void OnNextStage()
    {
        if (_currentIndex >= StageCount - 1) return;
        _currentIndex++;
        Refresh(true);
    }

    private void OnEnterStage()
    {
        StageTable stage = GetCurrentStage();
        if (stage == null) return;

        bool unlocked = IsCurrentStageUnlocked(stage);
        if (!unlocked) return;

        GameManager.Instance.SetCurrentStage(stage);
        UIManager.Instance.GoToStage();
    }

    private void OnBackToDungeonSelect()
    {
        gameObject.SetActive(false);
        UIManager.Instance.ChangeStateDungeonSelect();
    }

    private void OnOpenSetting()
    {
        UIManager.Instance.ChangeStateExitPanel();
    }

    private void Refresh(bool animate)
    {
        if (StageCount <= 0)
        {
            SetUnlockedView(false, null);
            SetLockedView(false, null);
            if (_btnStageEnter != null) _btnStageEnter.interactable = false;
            return;
        }

        StageTable stage = GetCurrentStage();
        if (stage == null) return;

        bool unlocked = IsCurrentStageUnlocked(stage);

        if (_btnPrevStage != null)
            _btnPrevStage.interactable = _currentIndex > 0;

        if (_btnNextStage != null)
            _btnNextStage.interactable = _currentIndex < StageCount - 1;

        if (_btnStageEnter != null)
            _btnStageEnter.interactable = unlocked;

        SetUnlockedView(unlocked, stage);
        SetLockedView(!unlocked, stage);
    }

    private StageTable GetCurrentStage()
    {
        if (_currentIndex < 0 || _currentIndex >= _towerStages.Count)
            return null;

        return _towerStages[_currentIndex];
    }

    private bool IsCurrentStageUnlocked(StageTable stage)
    {
        if (stage == null || GameManager.Instance == null) return false;
        return GameManager.Instance.IsStageUnlocked(stage.tower_id, stage.stage_index);
    }

    private void SetUnlockedView(bool active, StageTable stage)
    {
        if (_unlockedStagePanel != null)
            _unlockedStagePanel.SetActive(active);

        if (!active || stage == null) return;

        if (_txtStage != null)
            _txtStage.text = $"스테이지 {stage.stage_index}";

        // reward_preview를 나중에 실제 리소스 테이블과 연결
    }

    private void SetLockedView(bool active, StageTable stage)
    {
        if (_lockedStagePanel != null)
            _lockedStagePanel.SetActive(active);

        if (!active || stage == null) return;

        if (_txtLockedStage != null)
            _txtLockedStage.text = $"스테이지 {stage.stage_index}";

        if (_txtNeedStageReward != null)
        {
            int needStageIndex = Mathf.Max(1, stage.stage_index - 1);
            _txtNeedStageReward.text = $"스테이지 {needStageIndex} 클리어 필요";
        }
    }
}