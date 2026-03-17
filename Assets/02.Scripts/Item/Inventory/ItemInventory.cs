using System;
using System.Collections.Generic;
using UnityEngine;


public class ItemInventory : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;

    // Key: 아이템 , Value: 소지갯수
    private Dictionary<ItemInstance, int> _inventory;

    public Dictionary<ItemInstance, int> Inventory => _inventory;

    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;

    [ContextMenu("인벤토리 초기화")]
    public void ClearInventory()
    {
        _inventory.Clear();
        _onChangeInventory.OnStartEvent();
    }

    private void Awake()
    {
        _inventory = new Dictionary<ItemInstance, int>();
    }



    public void InitEvent()
    {
        OnAddItem = null;
        OnRemoveItem = null;
    }


    #region 장비 아이템


    // 장비 아이템 읽기
    public bool TryGetEquipment(int id, int enhanceLevel, EQUIP_GRADE grade, out ItemInstance item) => TryGetItem(id, enhanceLevel, grade, out item);
    public ItemInstance GetEquipment(int id, int enhanceLevel, EQUIP_GRADE grade) => GetItem(id, enhanceLevel, grade);


    // 장비 아이템 획득
    public void GainEquipment(int id, int enhanceLevel, EQUIP_GRADE grade, int amount = 1) =>
        GainItem(id, enhanceLevel, grade, amount);


    /// 장비 아이템 제거
    public void RemoveEquipment(int id, int enhanceLevel, EQUIP_GRADE grade, int amount = 1) =>
        UseItem(id, enhanceLevel, grade, amount);


    public void RemoveEquipment(ItemInstance item, int amount = 1) =>
        UseItem(item.ID, item.EnhanceLevel, item.Grade, amount);



    #endregion


    #region 유물 아이템


    /// 유물 아이템 읽기
    public bool TryGetRelic(int id, int enhanceLevel, out ItemInstance item) => 
        TryGetItem(id, enhanceLevel, EQUIP_GRADE.None, out item);
    public ItemInstance GetRelic(int id, int enhanceLevel) => GetItem(id, enhanceLevel, EQUIP_GRADE.None);


    // 유물 아이템 획득
    public void GainRelic(int id, int enhanceLevel)
    {
        var item = SearchItem(id, enhanceLevel, EQUIP_GRADE.None);

        // 아이템을 소지하고 있었을 때
        if (HasItem(item))
            Debug.LogWarning($"{id} 유물은 더이상 획득할 수 없습니다.");

        // 아이템을 소지하지 않았을 때
        else
            _inventory.Add(new ItemInstance(id, enhanceLevel, EQUIP_GRADE.None), ItemInstance.NoneStackAmount);

        _onChangeInventory.OnStartEvent();
    }


    // 유물 아이템 사용
    public void RemoveRelic(int id, int enhanceLevel)
    {
        var item = SearchItem(id, enhanceLevel, EQUIP_GRADE.None);
        if (!HasItem(item))
        { Debug.Log($"사용하려는 {id} 아이템을 소지하지 않았습니다."); return; }

        _inventory.Remove(item);
        _onChangeInventory.OnStartEvent();
    }


    #endregion


    #region 스택형 아이템(소모품, 강화 재료)


    // 스택형 아이템 읽기
    public bool TryGetStackItem(int id, out ItemInstance item) => TryGetItem(id, ItemInstance.NoneEnhanceLevel, EQUIP_GRADE.None, out item);
    public ItemInstance GetStackItem(int id) => GetItem(id, ItemInstance.NoneEnhanceLevel, EQUIP_GRADE.None);


    // 스택형 아이템 획득
    public void GainStackItem(int id, int amount = 1) =>
        GainItem(id, ItemInstance.NoneEnhanceLevel, EQUIP_GRADE.None, amount);


    // 스택형 아이템 사용
    public void UseStakItem(int id, int amount) =>
        UseItem(id, ItemInstance.NoneEnhanceLevel, EQUIP_GRADE.None, amount);


    #endregion



    #region 인벤토리 내부전용 메서드


    private bool TryGetItem(int id, int enhanceLevel, EQUIP_GRADE grad, out ItemInstance item)
    {
        item = SearchItem(id, enhanceLevel, grad);
        if(HasItem(item))
        {
            item.SetAmount(_inventory[item]);
            return true;
        }

        Debug.LogWarning($"{id} 아이템은 인벤토리에 없습니다.");
        return false;
    }

    // 인벤토리 내부전용 아이템 확인 메서드
    private ItemInstance GetItem(int id, int enhanceLevel, EQUIP_GRADE grad)
    {
        var item = SearchItem(id, enhanceLevel, grad);
        if (HasItem(item))
        {
            item.SetAmount(_inventory[item]);
            return item;
        }

        Debug.LogError($"{id} 아이템은 인벤토리에 없습니다.");
        return item;
    }


    // 인벤토리 내부전용 아이템 획득 메서드
    private void GainItem(int id, int enhanceLevel, EQUIP_GRADE grad, int amount)
    {
        var item = SearchItem(id, enhanceLevel, grad);

        if (HasItem(item))
            _inventory[item] += amount;

        else
            _inventory.Add(new ItemInstance(id, enhanceLevel, grad), amount);

        _onChangeInventory.OnStartEvent();
    }

    // 인벤토리 내부전용 아이템 사용 메서드
    private void UseItem(int id, int enhanceLevel, EQUIP_GRADE grade, int amount)
    {
        if (amount < 0)
        { Debug.LogError("사용하려는 값이 음수입니다."); return; }

        var item = SearchItem(id, enhanceLevel, grade);
        if (!HasItem(item))
        { Debug.Log($"사용하려는 {id} 아이템을 소지하지 않았습니다."); return; }

        if (_inventory[item] < amount)
        { Debug.Log($"사용량이 {id} 아이템 소지갯수를 초과합니다."); return; }

        _inventory[item] -= amount;

        if(_inventory[item] == 0)
            _inventory.Remove(item);
        _onChangeInventory.OnStartEvent();
    }


    // 인벤토리 내부전용 아이템 찾기 메서드
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

        return ItemInstance.None;
    }

    // 인벤토리 내부전용 아이템 사용 메서드
    private bool HasItem(ItemInstance invenItem)
    {
        return !(invenItem.ID == ItemInstance.None.ID &&
       invenItem.EnhanceLevel == ItemInstance.None.EnhanceLevel &&
       invenItem.Grade == ItemInstance.None.Grade);
    }


    #endregion
}
