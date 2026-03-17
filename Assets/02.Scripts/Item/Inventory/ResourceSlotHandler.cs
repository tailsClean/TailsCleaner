using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ResourceSlotHandler : InventorySlotHandler
{
    [Header("유물 강화 재료 슬롯")]
    [SerializeField] private UISlot _relicSlot;
    private ItemInstance _relicResourceItem;

    public override void UpdateInventory()
    {
        _relicResourceItem = default;
        int i = 0;
        foreach (var item in _items)
        {
            if (item.ID == ItemID.RelicReinforceResource)
                _relicResourceItem = item;
            else
                _slots[i++].SetSlot(item.ID, item.Amount);
        }
        for (; i < _slots.Count; i++)
        {
            _slots[i].Init();
        }

        if(_relicResourceItem.ID != default)
            _relicSlot.SetSlot(_relicResourceItem.ID, _relicResourceItem.Amount);
        else
            _relicSlot.Init();
    }
}
