using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StageSelect : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private Button _btnStageEnter;
    [SerializeField] private Button _btnMainEnter;
    [SerializeField] private Button _btnSetting;

    [Header("패널")]
    [SerializeField] private GameObject _unlockedStagePanel;
    [SerializeField] private GameObject _lockedStagePanel;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI _txtUnlockedStage;
    [SerializeField] private TextMeshProUGUI _txtLockedStage;
    [SerializeField] private TextMeshProUGUI _txtLockedNeedCondition;

    [Header("보상 슬롯")]
    [SerializeField] private Image[] _unlockedRewardIcons;
    [SerializeField] private Image[] _lockedRewardIcons;

    [Header("잠금 아이콘")]
    [SerializeField] private GameObject _lockedIconObject;

    private StageTableSO _stageTableSO;
    private List<StageTable> _towerStages = new List<StageTable>();

    // 1차 버전에서는 현재 보여줄 스테이지 1개만 관리
    private StageTable _currentDisplayStage;

    private int CurrentTowerId
    {
        get
        {
            if (GameManager.Instance == null || GameManager.Instance._currentTower == null)
                return 0;

            return GameManager.Instance._currentTower.tower_id;
        }
    }

    private bool _initialized;

    private void Start()
    {
        if (_btnStageEnter != null) _btnStageEnter.onClick.AddListener(OnEnterStage);
        if (_btnMainEnter != null) _btnMainEnter.onClick.AddListener(OnBackToDungeonSelect);
        if (_btnSetting != null) _btnSetting.onClick.AddListener(OnOpenSetting);

        _stageTableSO = DataManager.Instance.GetSOData<StageTableSO>();

        _initialized = true;

        BuildStageList();
        SelectInitialStage();
        Refresh();
    }

    private void OnEnable()
    {
        if (!_initialized) return;

        BuildStageList();
        SelectInitialStage();
        Refresh();
    }

    private void OnDestroy()
    {
        if (_btnStageEnter != null) _btnStageEnter.onClick.RemoveListener(OnEnterStage);
        if (_btnMainEnter != null) _btnMainEnter.onClick.RemoveListener(OnBackToDungeonSelect);
        if (_btnSetting != null) _btnSetting.onClick.RemoveListener(OnOpenSetting);
    }

    private void BuildStageList()
    {
        _towerStages.Clear();

        if (_stageTableSO == null)
        {
            Debug.LogError("[StageSelect] StageTableSO is null.");
            return;
        }

        int towerId = CurrentTowerId;
        if (towerId <= 0)
        {
            Debug.LogWarning("[StageSelect] Current tower is invalid.");
            return;
        }

        for (int i = 0; i < _stageTableSO.dataList.Count; i++)
        {
            StageTable stage = _stageTableSO.dataList[i];
            if (stage.tower_id == towerId)
            {
                _towerStages.Add(stage);
            }
        }

        _towerStages.Sort((a, b) => a.stage_index.CompareTo(b.stage_index));

        Debug.Log($"[StageSelect] towerId={towerId}, stageCount={_towerStages.Count}");
    }

    private void SelectInitialStage()
    {
        _currentDisplayStage = null;

        if (_towerStages.Count == 0)
            return;

        // 기본은 가장 먼저 입장 가능한 스테이지를 보여준다.
        for (int i = 0; i < _towerStages.Count; i++)
        {
            if (IsUnlocked(_towerStages[i]))
            {
                _currentDisplayStage = _towerStages[i];
                return;
            }
        }

        // 전부 잠겨 있으면 첫 스테이지 표시
        _currentDisplayStage = _towerStages[0];
    }

    private void Refresh()
    {
        if (_currentDisplayStage == null)
        {
            SetUnlockedView(false, null);
            SetLockedView(false, null);
            if (_btnStageEnter != null) _btnStageEnter.interactable = false;
            return;
        }

        bool unlocked = IsUnlocked(_currentDisplayStage);

        SetUnlockedView(unlocked, _currentDisplayStage);
        SetLockedView(!unlocked, _currentDisplayStage);

        if (_btnStageEnter != null)
            _btnStageEnter.interactable = unlocked;
    }

    private bool IsUnlocked(StageTable stage)
    {
        if (stage == null || GameManager.Instance == null)
            return false;

        return GameManager.Instance.IsStageUnlocked(stage.tower_id, stage.stage_index);
    }

    private void SetUnlockedView(bool active, StageTable stage)
    {
        if (_unlockedStagePanel != null)
            _unlockedStagePanel.SetActive(active);

        if (!active || stage == null) return;

        if (_txtUnlockedStage != null)
            _txtUnlockedStage.text = $"스테이지 {stage.stage_index}";

        BindRewardIcons(_unlockedRewardIcons, stage.reward_preview);

        if (_lockedIconObject != null)
            _lockedIconObject.SetActive(false);
    }

    private void SetLockedView(bool active, StageTable stage)
    {
        if (_lockedStagePanel != null)
            _lockedStagePanel.SetActive(active);

        if (!active || stage == null) return;

        if (_txtLockedStage != null)
            _txtLockedStage.text = $"스테이지 {stage.stage_index}";

        if (_txtLockedNeedCondition != null)
        {
            if (stage.stage_index <= 1)
            {
                int prevTowerId = Mathf.Max(1, stage.tower_id - 1);
                _txtLockedNeedCondition.text = $"{prevTowerId}번 탑 마지막 스테이지 클리어 필요";
            }
            else
            {
                _txtLockedNeedCondition.text = $"스테이지 {stage.stage_index - 1} 클리어 필요";
            }
        }

        BindRewardIcons(_lockedRewardIcons, stage.reward_preview);

        if (_lockedIconObject != null)
            _lockedIconObject.SetActive(true);
    }

    private void BindRewardIcons(Image[] rewardIcons, int rewardPreviewId)
    {
        if (rewardIcons == null) return;

        for (int i = 0; i < rewardIcons.Length; i++)
        {
            if (rewardIcons[i] == null) continue;

            // 아직 reward_preview와 실제 리소스 연결 테이블이 없으므로 임시로 표시만 유지
            rewardIcons[i].gameObject.SetActive(true);
            rewardIcons[i].sprite = null;
        }
    }

    private void OnEnterStage()
    {
        if (_currentDisplayStage == null)
        {
            Debug.LogError("[StageSelect] 현재 선택된 스테이지 없음");
            return;
        }

        if (!IsUnlocked(_currentDisplayStage))
        {
            Debug.LogWarning("[StageSelect] 잠긴 스테이지");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[StageSelect] GameManager 없음");
            return;
        }

        GameManager.Instance.SetCurrentStage(_currentDisplayStage);

        Debug.Log($"[StageSelect] Enter Stage → stageId={_currentDisplayStage.stage_id}");

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

    // 나중에 스크롤 뷰 붙을 때 외부에서 호출할 수 있도록 남겨둠
    public void SetDisplayStageByIndex(int stageIndex)
    {
        for (int i = 0; i < _towerStages.Count; i++)
        {
            if (_towerStages[i].stage_index == stageIndex)
            {
                _currentDisplayStage = _towerStages[i];
                Refresh();
                return;
            }
        }

        Debug.LogWarning($"[StageSelect] stageIndex not found: {stageIndex}");
    }

    public void SetDisplayStage(StageTable stage)
    {
        _currentDisplayStage = stage;
        Refresh();
    }

    public StageTable GetCurrentDisplayStage()
    {
        return _currentDisplayStage;
    }
}