using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelect : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform _contentRoot;       // stageVerticalLayout
    [SerializeField] private StageSelectPanel _panelPrefab; // StagePanel prefab

    [Header("Buttons")]
    [SerializeField] private Button _btnBack;
    [SerializeField] private Button _btnSetting;

    private StageTableSO _stageTableSO;
    private readonly List<StageTable> _towerStages = new List<StageTable>();

    private void Start()
    {
        _stageTableSO = DataManager.Instance.GetSOData<StageTableSO>();

        if (_btnBack != null)
            _btnBack.onClick.AddListener(OnBack);

        if (_btnSetting != null)
            _btnSetting.onClick.AddListener(OnOpenSetting);

        Refresh();
    }

    private void OnEnable()
    {
        if (_stageTableSO != null)
            Refresh();
    }

    private void OnDestroy()
    {
        if (_btnBack != null)
            _btnBack.onClick.RemoveListener(OnBack);

        if (_btnSetting != null)
            _btnSetting.onClick.RemoveListener(OnOpenSetting);
    }

    private void Refresh()
    {
        if (_contentRoot == null || _panelPrefab == null || _stageTableSO == null)
            return;

        ClearItems();
        BuildStageList();
        CreateItems();
    }

    private void ClearItems()
    {
        for (int i = _contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_contentRoot.GetChild(i).gameObject);
        }
    }

    private int CurrentTowerId
    {
        get
        {
            if (GameManager.Instance == null || GameManager.Instance._currentTower == null)
                return 0;

            return GameManager.Instance._currentTower.tower_id;
        }
    }

    private void BuildStageList()
    {
        _towerStages.Clear();

        int towerId = CurrentTowerId;
        if (towerId <= 0) return;

        foreach (var stage in _stageTableSO.dataList)
        {
            if (stage.tower_id == towerId)
                _towerStages.Add(stage);
        }

        _towerStages.Sort((a, b) => a.stage_index.CompareTo(b.stage_index));
    }

    private void CreateItems()
    {
        for (int i = 0; i < _towerStages.Count; i++)
        {
            StageTable stage = _towerStages[i];
            bool unlocked = IsStageUnlocked(stage);

            StageSelectPanel panel = Instantiate(_panelPrefab, _contentRoot);
            panel.SetData(stage, unlocked, OnClickStage);
        }
    }

    private bool IsStageUnlocked(StageTable stage)
    {
        if (stage == null || GameManager.Instance == null || _stageTableSO == null)
            return false;

        int towerId = stage.tower_id;
        int stageIndex = stage.stage_index;

        // 1번 탑 1스테이지는 항상 열림
        if (towerId == 5001 && stageIndex == 1)
            return true;

        // 같은 탑 내부
        if (stageIndex > 1)
        {
            return GameManager.Instance.GetMaxClearStageIndex(towerId) >= stageIndex - 1;
        }

        // 이전 탑 마지막 스테이지를 클리어해야 다음 탑 1스테이지 오픈
        int prevTowerId = towerId - 1;
        int lastStageIndex = GetLastStageIndex(prevTowerId);

        return GameManager.Instance.GetMaxClearStageIndex(prevTowerId) >= lastStageIndex;
    }

    private int GetLastStageIndex(int towerId)
    {
        int max = 0;

        foreach (var stage in _stageTableSO.dataList)
        {
            if (stage.tower_id == towerId)
                max = Mathf.Max(max, stage.stage_index);
        }

        return max;
    }

    private void OnClickStage(StageTable stage)
    {
        if (stage == null) return;
        if (!IsStageUnlocked(stage)) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetCurrentStage(stage);
        UIManager.Instance.GoToStage();
    }

    private void OnBack()
    {
        gameObject.SetActive(false);
        UIManager.Instance.ChangeStateDungeonSelect();
    }

    private void OnOpenSetting()
    {
        UIManager.Instance.ChangeStateSettingPanel();
    }
}