using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 케이스별 판단 메서드 만들어서 코드 좀 줄이자
public class Inventory : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;

    // Key: 아이템ID , Value: 소지갯수
    private Dictionary<int, int> _equipInventory;
    private Dictionary<int, int> _relicInventory;
    private List<RelicStatus> _relicStatus;

    private Dictionary<int, int> _reinforceResourceInventory;
    private Dictionary<int, int> _consumeInventory;


    public Dictionary<int, int> EquipInventory => _equipInventory;
    public Dictionary<int, int> RelicInventory => _relicInventory;

    public List<RelicStatus> RelicStatus => _relicStatus;

    public Dictionary<int, int> ReinforceResourceInventory => _reinforceResourceInventory;
    public Dictionary<int, int> ConsumeInventory => _consumeInventory;


    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;



    private void Awake()
    {
        _equipInventory = new Dictionary<int, int>();
        _relicInventory = new Dictionary<int, int>();
        _relicStatus = new List<RelicStatus>();
        _reinforceResourceInventory = new Dictionary<int, int>();
        _consumeInventory = new Dictionary<int, int>();
    }


    // 새로운 유믈 조회 교체
    public void SetRelic(RelicStatus newRelic)
    {
        for (int i = 0; i < _relicStatus.Count; i++)
        {
            if (_relicStatus[i].InstanceID == newRelic.InstanceID)
            {
                _relicStatus[i] = newRelic;
                _onChangeInventory.OnStartEvent();
                return;
            }
        }
        _relicStatus.Add(newRelic);
        _onChangeInventory.OnStartEvent();
    }
    // 인벤토리 소지한 유물 정보 반환
    public RelicStatus GetRelicInfo(int id, int enhanceLevel)
    {
        foreach (var relic in _relicStatus)
        {
            if (relic.UniqueID == id && relic.EnhanceLevel == enhanceLevel)
                return new RelicStatus(relic.InstanceID, id, enhanceLevel);
        }
        return default;
    }

    // 인벤토리 소지 아이템 정보 반환
    public ItemStack GetItemInfo(int id)
    {
        var item = ItemDB.GetItemData<ItemBaseSO>(id);

        switch (item.ItemType)
        {
            case ITEM_TYPE.Equipment:
                return new ItemStack(item, _equipInventory[id]);

            case ITEM_TYPE.Relic:
                return new ItemStack(item, _relicInventory[id]);

            case ITEM_TYPE.Reinforcement:
                return new ItemStack(item, GetAmount(_reinforceResourceInventory, id));

            case ITEM_TYPE.Consume:
                return new ItemStack(item, _consumeInventory[id]);

            default:
                return default;
        }
    }
    private int? GetAmount(Dictionary<int, int> inventory, int id)
    {
        if(inventory.TryGetValue(id, out var value))
            return value;
        return null;
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

// 유물 상태 구조체
public struct RelicStatus
{
    public int InstanceID;
    public int UniqueID;
    public int EnhanceLevel;

    public RelicStatus(int instanceID, int id, int enhanceLevel)
    {
        InstanceID = instanceID;
        UniqueID = id;
        EnhanceLevel = enhanceLevel;
    }

    public override bool Equals(object obj)
    {
        if (obj is RelicStatus other)
        {
            return UniqueID == other.UniqueID &&
                   EnhanceLevel == other.EnhanceLevel;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueID, EnhanceLevel);
    }
}
