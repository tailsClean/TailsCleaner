using System.Collections.Generic;
using UnityEngine;

public class TestReward: MonoBehaviour
{
    public ItemInventory inventory;
    public ItemCurrency currency;

    [Header("보기 전용(수정해도 의미 없음")]
    public int _stageGroupID;
    public RewardDTO _currentReward;
    private RewardSystem _rewardSystem;

    [Header("=========================================================")]
    [Header("보상이 인벤토리에 들어오는지 확인을 위해서 체크\n(기존 인벤토리를 초기화 해버림)")]
    public bool IsCheckingInventory = false;

    [Header("인벤토리")]
    public int Gold;
    public List<ItemInstance> items = new();
    public List<int> itemAmount = new();

    

    private void Awake()
    {
        _rewardSystem = new RewardSystem();
        _currentReward = _rewardSystem.CurrentReward;
    }

    private void Start()
    {
        if (ItemManager.Instance != null)
        {
            inventory = ItemManager.Instance.Inventory;
            currency = ItemManager.Instance.Currency;
        }
    }

    private void Update()
    {
        if (IsCheckingInventory)
        {
            Gold = currency.GoldAmount;

            items.Clear();
            itemAmount.Clear();
            foreach (var item in inventory.Inventory)
            {
                items.Add(item.Key);
                itemAmount.Add(item.Value);
            }
        }
    }


    [ContextMenu("리워드 보상 호출")]
    public void OnGainReward()
    {
        _rewardSystem.OnGainReward(_stageGroupID);
    }
}
