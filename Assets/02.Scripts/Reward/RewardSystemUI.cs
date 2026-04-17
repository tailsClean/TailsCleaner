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

    private bool _initialized;

    private void Awake()
    {
        EnsureInitialized();
    }

    private void OnEnable()
    {
        EnsureInitialized();

        _rewardSystem.OnReward -= RefreshUI;
        _rewardSystem.OnReward += RefreshUI;

        if (_gainButton != null)
        {
            _gainButton.onClick.RemoveListener(SetRewardToInventory);
            _gainButton.onClick.AddListener(SetRewardToInventory);
        }
    }

    private void OnDisable()
    {
        if (_rewardSystem != null)
            _rewardSystem.OnReward -= RefreshUI;

        if (_gainButton != null)
            _gainButton.onClick.RemoveListener(SetRewardToInventory);
    }

    // [추가] RewardSystem 보장 초기화
    private void EnsureInitialized()
    {
        if (_initialized && _rewardSystem != null)
            return;

        _rewardSystem = new RewardSystem();
        _currentReward = _rewardSystem.CurrentReward;
        _initialized = true;
    }

    public void ShowReward(int stageGroupID)
    {
        EnsureInitialized(); // 비활성 패널이었다가 바로 호출돼도 안전

        _stageGroupID = stageGroupID;
        _rewardSystem.OnGainReward(_stageGroupID);
        _currentReward = _rewardSystem.CurrentReward;

        if (_currentReward == null)
        {
            Debug.LogError($"[RewardSystemUI] CurrentReward is null. stageGroupID={_stageGroupID}");
            InitAllSlots();
            SetGoldText();
            return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_rewardSystem == null)
        {
            Debug.LogError("[RewardSystemUI] _rewardSystem is null in RefreshUI.");
            return;
        }

        _currentReward = _rewardSystem.CurrentReward;
        SetGoldText();
        SetItems();
    }

    private void SetGoldText()
    {
        if (_goldAmountText == null)
        {
            Debug.LogError("[RewardSystemUI] _goldAmountText is null.");
            return;
        }

        int gold = _currentReward != null ? _currentReward.GoldAmount : 0;
        _goldAmountText.text = $"{gold} 골드";
    }

    private void SetItems()
    {
        if (_itemSlots == null) return;

        if (_currentReward == null || _currentReward.Items == null)
        {
            InitAllSlots();
            return;
        }

        int i = 0;
        foreach (var item in _currentReward.Items)
        {
            if (i >= _itemSlots.Count)
                break;

            _itemSlots[i++].SetSlot(item);
        }

        for (; i < _itemSlots.Count; i++)
        {
            _itemSlots[i].Init();
        }
    }

    [ContextMenu("리워드 보상 호출")]
    public void OnGainReward()
    {
        EnsureInitialized();

        _rewardSystem.OnGainReward(_stageGroupID);
        _currentReward = _rewardSystem.CurrentReward;
        RefreshUI();
    }

    public void SetRewardToInventory()
    {
        EnsureInitialized();
        _rewardSystem.SetRewardToInventory();
    }

    private void InitAllSlots()
    {
        if (_itemSlots == null) return;

        for (int i = 0; i < _itemSlots.Count; i++)
        {
            _itemSlots[i].Init();
        }
    }
}