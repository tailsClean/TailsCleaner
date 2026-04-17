using System.Collections.Generic;
using UnityEngine;


public class EquipSlotHandler : InventorySlotHandler
{
    private Dictionary<PART, UISlotBundle> _equipDict;

    protected override void Awake()
    {
        base.Awake();
        SetDict();
    }

    public override void UpdateInventory()
    {
        foreach(var bundle in _equipDict.Values)
        {
            bundle.Init();
        }

        foreach (var item in _items)
        {
            if (!ItemDB.TryGetData<MaterialEquipData>(item.ID, out var equipData))
            { Debug.Log("장비 아이템 매칭 실패"); continue; }

            SetSlots(equipData.EquipMatter.part_type, item);
        }

        foreach (var bundle in _equipDict.Values)
        {
            bundle.SetSlots();
        }
    }



    #region 내부 메서드

    private void SetDict()
    {
        _equipDict = new Dictionary<PART, UISlotBundle>();
        for(int i = 0; i < _slots.Count; i++)
        {
            int index = i / 7;
            switch(index)
            {
                case 0:
                    AddEquipDict(PART.Weapon, _slots[i]);
                    break;

                case 1:
                    AddEquipDict(PART.Helmet, _slots[i]);
                    break;

                case 2:
                    AddEquipDict(PART.Cloak, _slots[i]);
                    break;

                case 3:
                    AddEquipDict(PART.Shoes, _slots[i]);
                    break;
            }
        }
    }
    private void AddEquipDict(PART part, UISlot slot)
    {
        if (!_equipDict.TryGetValue(part, out var equip))
            _equipDict.Add(part, new UISlotBundle(slot, _onItemPopup));

        else
            _equipDict[part].AddSlot(slot);
    }

    private void SetSlots(PART part, ItemInstance item)
    {

        switch(part)
        {
            case PART.Weapon:
                _equipDict[PART.Weapon].SetItems(item);
                break;

            case PART.Helmet:
                _equipDict[PART.Helmet].SetItems(item); 
                break;

            case PART.Cloak:
                _equipDict[PART.Cloak].SetItems(item); 
                break;

            case PART.Shoes:
                _equipDict[PART.Shoes].SetItems(item); 
                break;
        }
    }


    #endregion


    // 인벤토리 아이템에 순서 부여
    //private void ItemIdOrderring()
    //{
    //    foreach (var id in _inventory.Keys)
    //    {
    //        _itemOrder.Add(id);
    //    }
    //}


    public class UISlotBundle
    {
        public List<UISlot> slots;


        private List<ItemInstance> _items;
        private ItemInstanceEventChannelSO _onItemPopup;

        public UISlotBundle(UISlot slot, ItemInstanceEventChannelSO onItemPopup)
        {
            slots = new List<UISlot>();
            _items = new List<ItemInstance>();
            slots.Add(slot);
            _onItemPopup = onItemPopup;
        }

        public void AddSlot(UISlot slot)
        {
            slots.Add(slot);
        }

        public void Init() => _items.Clear();
        public void SetItems(ItemInstance item) => _items.Add(item);


        public void SetSlots()
        {
            int i = 0;
            foreach(var item in _items)
            {
                slots[i].Init();
                slots[i].AddListener(() => _onItemPopup.OnStartEvent(item));
                slots[i++].SetSlot(item, item.Amount);
            }

            for (; i < slots.Count; i++)
            {
                slots[i].Init();
            }
        }
    }
}
