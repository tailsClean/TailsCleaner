using System.Collections.Generic;
using UnityEngine;

public class RewardDistributor: MonoBehaviour
{
    public int _stageGroupID;
    public ItemInventory inventory;
    public Currency currency;

    [Header("보기 전용(수정해도 의미 없음")]
    public RewardDTO _currentReward;
    private RewardSystem _rewardSystem;
    [Header("=========================================================")]
    [Header("인벤토리")]
    public int Gold;
    public List<ItemInstance> items = new();
    public List<int> itemAmount = new();



    private void Awake()
    {
        _rewardSystem = new RewardSystem();
        _currentReward = _rewardSystem.CurrentReward;
    }

    private void Update()
    {
        Gold = currency.GoldAmount;
        items.Clear();
        itemAmount.Clear();
        foreach(var item in inventory.Inventory)
        {
            items.Add(item.Key);
            itemAmount.Add(item.Value);
        }
    }


    [ContextMenu("리워드 보상 호출")]
    public void OnGainReward()
    {
        _rewardSystem.OnGainReward(_stageGroupID);
    }

    [ContextMenu("가방에 넣자")]
    public void SetRewardToInventory()
    {
        if(_currentReward == null ) 
        { Debug.LogError("인벤토리에 추가할 보상 목록이 없습니다."); return; }

        currency.GainGold(_currentReward.GoldAmount);

        foreach(var reward in _currentReward.Items)
        {
            GainItem(reward);
        }
    }

    private void GainItem(ItemInstance item)
    {
        switch(item.ItemType)
        {
            // 얻는 System 인게임 보상은 골드뿐
            case ITEM_TYPE.Equipment:
                inventory.GainEquipment(item.ID, item.Grade, item.Amount);
                break;

            case ITEM_TYPE.Reinforcement:
                inventory.GainStackItem(item.ID, item.Amount);
                break;
        }
    }
}
