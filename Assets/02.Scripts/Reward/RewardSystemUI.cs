using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardSystemUI: MonoBehaviour
{
    [Header("리워드UI 필드")]
    [SerializeField] private List<UISlot> _itemSlots;
    [SerializeField] private TextMeshProUGUI _goldAmountText;
    [SerializeField] private Button _gainButton;

    private RewardSystem _rewardSystem;

    [Header("=========================================================")]
    [Header("리워드시스템 내부 확인용")]
    public int _stageGroupID;
    public RewardDTO _currentReward;

    

    private void Awake()
    {
        _rewardSystem = new RewardSystem();
        _currentReward = _rewardSystem.CurrentReward;
        _gainButton.onClick.AddListener(SetRewardToInventory);
    }

    private void OnEnable()
    {
        _rewardSystem.OnReward += SetGoldText;
        _rewardSystem.OnReward += SetItems;
    }

    private void OnDisable()
    {
        _rewardSystem.OnReward -= SetGoldText;
        _rewardSystem.OnReward -= SetItems;
        _gainButton.onClick.RemoveListener(SetRewardToInventory);
    }


    private void SetGoldText() => _goldAmountText.text = $"{_currentReward.GoldAmount} 골드";

    private void SetItems()
    {
        if (_currentReward == null || _currentReward.Items == null)
            return;

        int i = 0;
        foreach(var item in _currentReward.Items)
        {
            _itemSlots?[i++].SetSlot(item);
        }
        for (; i < _itemSlots.Count; i++)
        {
            _itemSlots[i].Init();
        }
    }


    [ContextMenu("리워드 보상 호출")]
    public void OnGainReward()
    {
        //_stageGroupID = _rewardSystem.StageGroupID;
        _rewardSystem.OnGainReward(_stageGroupID);
    }


    public void SetRewardToInventory() => _rewardSystem.SetRewardToInventory();
}
