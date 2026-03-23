using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardSystem
{
    private ItemInventory _inventory;
    private ItemCurrency _currency;

    private RewardDTO _currentReward;
    private RewardCreator _rewardSystem;

    public int StageGroupID => _rewardSystem.StageGroupID;
    public RewardDTO CurrentReward => _currentReward;

    public event Action OnReward;

    /// <summary>
    /// 전역 데이터 참조를 위해 Start에서 생성자 호출해야 함
    /// </summary>
    public RewardSystem()
    {
        _rewardSystem = new RewardCreator();
        _currentReward = _rewardSystem.CurrentReward;
        _inventory = ItemManager.Instance.Inventory;
        _currency = ItemManager.Instance.Currency;
    }


    // 리워드 보상 호출
    public void OnGainReward(int stageGroupID)
    {
        _rewardSystem.OnGainReward(stageGroupID);

        OnReward?.Invoke();
    }

    // 현재 보상목록을 인벤토리에 추가하는 메서드
    public void SetRewardToInventory()
    {
        if(_currentReward == null ) 
        { Debug.LogError("인벤토리에 추가할 보상 목록이 없습니다."); return; }

        _currency.GainGold(_currentReward.GoldAmount);

        foreach(var reward in _currentReward.Items)
        {
            GainItem(reward);
        }
    }

    // 아이템의 타입을 선별해서 인벤토라에 넣기
    private void GainItem(ItemInstance item)
    {
        switch(item.ItemType)
        {
            // 얻는 System 인게임 보상은 골드뿐
            case ITEM_TYPE.Equipment:
                _inventory.GainEquipment(item.ID, item.Grade, item.Amount);
                break;

            case ITEM_TYPE.Reinforcement:
                _inventory.GainStackItem(item.ID, item.Amount);
                break;
        }
    }
}
