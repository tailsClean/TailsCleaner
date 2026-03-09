using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    // Key: 아이템ID , Value: 소지갯수
    private Dictionary<int, int> _equipInventory;
    private Dictionary<int, int> _relicInventory;
    private Dictionary<int, int> _reinforceResourceInventory;
    private Dictionary<int, int> _consumeInventory;


    public Dictionary<int, int> EquipInventory => _equipInventory;
    public Dictionary<int, int> RelicInventory => _relicInventory;
    public Dictionary<int, int> ReinforceResourceInventory => _reinforceResourceInventory;
    public Dictionary<int, int> ConsumeInventory => _consumeInventory;


    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;



    private void Awake()
    {
        _equipInventory = new Dictionary<int, int>();
        _relicInventory = new Dictionary<int, int>();
        _reinforceResourceInventory = new Dictionary<int, int>();
        _consumeInventory = new Dictionary<int, int>();
    }





    // 인벤토리 아이템 관리용 정보 반환
    public ItemBaseSO GetItem(int id) => ItemDB.GetItemSO<ItemBaseSO>(id);


    public void InitEvent()
    {
        OnAddItem = null;
        OnRemoveItem = null;
    }


    #region 아이템 획득시, 인벤토리 저장
    public void GainItem(ITEM_TYPE itemType, int id, int amount = 1)
    {
        switch (itemType)
        {
            case ITEM_TYPE.Equipment:
                GainItem(_equipInventory, id, amount);
                break;

            case ITEM_TYPE.Relic:
                GainItem(_relicInventory, id, amount);
                break;

            case ITEM_TYPE.Reinforcement:
                GainItem(_reinforceResourceInventory, id, amount);
                break;

            case ITEM_TYPE.Consume:
                GainItem(_consumeInventory, id, amount);
                break;
        }
    }
    private void GainItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (inventory.TryGetValue(id, out var item))
            inventory[id] += amount;

        else
        {
            inventory.Add(id, amount);
            OnAddItem?.Invoke(id);
        }
    }
    #endregion


    #region 인벤토리의 아이템 사용
    public void UseItem(ITEM_TYPE itemType, int id, int amount = 1)
    {
        switch (itemType)
        {
            case ITEM_TYPE.Equipment:
                UseItem(_equipInventory, id, amount);
                break;

            case ITEM_TYPE.Relic:
                UseItem(_relicInventory, id, amount);
                break;

            case ITEM_TYPE.Reinforcement:
                UseItem(_reinforceResourceInventory, id, amount);
                break;

            case ITEM_TYPE.Consume:
                UseItem(_consumeInventory, id, amount);
                break;
        }
    }
    private void UseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        { Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>"); return; }

        if (itemCount > amount)
            inventory[id] -= amount;

        else if (itemCount == amount)
        {
            inventory.Remove(id);
            OnRemoveItem?.Invoke(id);
        }

        else
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
    }
    #endregion


    #region 아이템 사용가능 여부
    public bool TryUseItem(ITEM_TYPE itemType, int id, int amount = 1)
    {
        switch(itemType)
        {
            case ITEM_TYPE.Equipment:
                return TryUseItem(_equipInventory, id, amount);
                
            case ITEM_TYPE.Relic: 
                return TryUseItem(_relicInventory, id, amount);
            
            case ITEM_TYPE.Reinforcement: 
                return TryUseItem(_reinforceResourceInventory, id, amount);
            
            case ITEM_TYPE.Consume: 
                return TryUseItem(_consumeInventory, id, amount);
        }
        return false;
    }
    private bool TryUseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        Debug.Log(inventory);
        if(!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return false;
        }

        if(itemCount < amount)
        {
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
            return false;
        }

        return true;
    }
    #endregion
}