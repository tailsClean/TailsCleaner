using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageSelectPanel : MonoBehaviour
{
    [Header("Unlocked Panel")]
    [SerializeField] private GameObject _unlockedRoot;
    [SerializeField] private TextMeshProUGUI _txtUnlockedStage;
    [SerializeField] private Image _imgReward1;
    [SerializeField] private Image _imgReward2;
    [SerializeField] private Image _imgReward3;
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
        BindRewards(stage.reward_preview);
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

    private void BindRewards(int rewardPreviewId)
    {
        // 아직 reward_preview와 실제 아이콘 연결 전이므로,
        // 현재는 오브젝트만 유지하고 sprite는 비워둠.
        if (_imgReward1 != null) _imgReward1.gameObject.SetActive(true);
        if (_imgReward2 != null) _imgReward2.gameObject.SetActive(true);
        if (_imgReward3 != null) _imgReward3.gameObject.SetActive(true);

        if (_imgReward1 != null) _imgReward1.sprite = null;
        if (_imgReward2 != null) _imgReward2.sprite = null;
        if (_imgReward3 != null) _imgReward3.sprite = null;
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