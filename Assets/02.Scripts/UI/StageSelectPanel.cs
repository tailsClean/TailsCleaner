using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageSelectPanel : MonoBehaviour
{
    [Header("Unlocked Panel")]
    [SerializeField] private GameObject _unlockedRoot;
    [SerializeField] private TextMeshProUGUI _txtUnlockedStage;
    [SerializeField] private StageRewardUI _rewardPreviewUI;
    [SerializeField] private Button _btnStageEnter;

    [Header("Locked Panel")]
    [SerializeField] private GameObject _lockedRoot;
    [SerializeField] private TextMeshProUGUI _txtLockedStage;
    [SerializeField] private Image _imgLockIcon;

    private StageTable _stageData;
    private Action<StageTable> _onClickStage;

    public void SetData(StageTable stage, bool unlocked, Action<StageTable> onClickStage)
    {
        _stageData = stage;
        _onClickStage = onClickStage;

        SetState(unlocked);
        SetTexts(stage);
        BindRewards(stage);
        BindButton(unlocked);
    }

    private void SetState(bool unlocked)
    {
        if (_unlockedRoot != null)
            _unlockedRoot.SetActive(unlocked);

        if (_lockedRoot != null)
            _lockedRoot.SetActive(!unlocked);
    }

    private void SetTexts(StageTable stage)
    {
        string stageText = $"스테이지 {stage.stage_index}";

        if (_txtUnlockedStage != null)
            _txtUnlockedStage.text = stageText;

        if (_txtLockedStage != null)
            _txtLockedStage.text = stageText;
    }

    private void BindRewards(StageTable stage)
    {
        if (_rewardPreviewUI == null)
        {
            Debug.LogWarning($"[StageSelectPanel] StageRewardUI is not assigned. stage={stage.stage_index}");
            return;
        }

        int rewardGroupId = stage.reward_group_id;

        if (rewardGroupId <= 0)
        {
            Debug.LogWarning($"[StageSelectPanel] Invalid reward_group_id={rewardGroupId}, stage={stage.stage_index}");
            return;
        }

        Debug.Log($"[StageSelectPanel] BindRewards stage={stage.stage_index}, reward_group_id={rewardGroupId}");
        _rewardPreviewUI.SetSlots(rewardGroupId);
    }

    private void BindButton(bool unlocked)
    {
        if (_btnStageEnter == null) return;

        _btnStageEnter.onClick.RemoveAllListeners();
        _btnStageEnter.interactable = unlocked;

        if (unlocked)
        {
            _btnStageEnter.onClick.AddListener(() =>
            {
                _onClickStage?.Invoke(_stageData);
            });
        }
    }
}