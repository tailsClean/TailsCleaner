using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 케이스별 판단 메서드 만들어서 코드 좀 줄이자
public class ItemInventory : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;

    private Dictionary<ItemInstance, int> _inventory;

    public Dictionary<ItemInstance, int> Inventory => _inventory;

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
        _inventory = new Dictionary<ItemInstance, int>();
        _equipInventory = new Dictionary<int, int>();
        _relicInventory = new Dictionary<int, int>();
        _reinforceResourceInventory = new Dictionary<int, int>();
        _consumeInventory = new Dictionary<int, int>();
    }


    // 추가 수정 필요
    public void RemoveEquipment(ItemInstance item)
    {
        foreach (var invenItem in _inventory)
        {
            if (HasItem(invenItem.Key, item))
            {
                _inventory.Remove(invenItem.Key);
                _onChangeInventory.OnStartEvent();
                return;
            }
        }
        Debug.Log("아이템 제거 실패");
    }


    /// <summary>
    /// 꺼냈던 아이템을 다시 인벤토리에 넣는 메서드
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseItem(ItemInstance item)
    {
        foreach(var invenItem in _inventory)
        {
            if (HasItem(invenItem.Key, item))
            {
                if (item.Amount == ItemInstance.NoneStackAmount)
                    _inventory[item] = item.Amount;
                else
                    _inventory[item] += item.Amount;

                _onChangeInventory.OnStartEvent();
                return;
            }
        }
        _inventory.Add(item, item.Amount);
        _onChangeInventory.OnStartEvent();
    }

    private bool HasItem(ItemInstance invenItem, ItemInstance item)
    {
        return invenItem.ID == item.ID &&
               invenItem.EnhanceLevel == item.EnhanceLevel &&
               invenItem.Grade == item.Grade;
    }

    /// <summary>
    /// 일반 스택형 아이템 반환
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ItemInstance GetItem(int id)
    {
        var item = SearchItem(id, ItemInstance.NoneEnhanceLevel, EQUIP_GRADE.None);
        item.SetAmount(_inventory[item]);
        return item;
    }

    /// <summary>
    /// 유물 아이템 반환
    /// </summary>
    /// <param name="id"></param>
    /// <param name="enhanceLevel"></param>
    /// <returns></returns>
    public ItemInstance GetItem(int id, int enhanceLevel)
    {
        var item = SearchItem(id,enhanceLevel, EQUIP_GRADE.None);
        item.SetAmount(ItemInstance.NoneStackAmount);
        return item;
    }

    private ItemInstance SearchItem(int id, int enhanceLevel, EQUIP_GRADE grade)
    {
        foreach (var item in _inventory)
        {
            bool isItem = item.Key.ID == id &&
                          item.Key.EnhanceLevel == enhanceLevel &&
                          item.Key.Grade == grade;
            if (isItem)
                return item.Key;
        }

        Debug.Log($"{id}에 해당하는 아이템이 인벤토리에 없습니다.");
        return ItemInstance.None;
    }


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
        _onChangeInventory.OnStartEvent();
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
        _onChangeInventory.OnStartEvent();
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
        switch (itemType)
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
        if (!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return false;
        }

        if (itemCount < amount)
        {
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
            return false;
        }

        return true;
    }
    #endregion
}
