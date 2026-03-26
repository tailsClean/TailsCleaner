using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CraftingSystem))]
public class CraftingSystemUI : UIGroup
{
    private CraftingSystem _craftingSystem;
    private PlayerLoadout _loadout;
    private ItemInventory _inventory;


    [Header("선택된 합성 장비")]
    [SerializeField] private UISlot _mainEquipSlot;
    [SerializeField] private List<UISlot> _resourceCostSlots;

    [Header("장비 선택리스트")]
    [SerializeField] private List<UISlot> _loadoutSlots;
    [SerializeField] private List<UISlot> _resourceEquipSlots;



    private void Awake()
    {
        _craftingSystem = GetComponent<CraftingSystem>();
    }

    protected override void Start()
    {
        base.Start();
        _inventory = _craftingSystem.UsingInventory;
        _loadout = ItemManager.Instance.Loadout;
    }

    private void Update()
    {
        SetMainEquipUI();
        SetResourceEquipsUI();

        SetLoadoutUI();
        SetInventoryEquipUI();
    }

    #region 선택된 장비창

    // 등급업 장비UI 세팅
    private void SetMainEquipUI()
    {
        if(_craftingSystem.MainEquip == null)
        {
            _mainEquipSlot.Init();
            return;
        }

        var mainEquip = _craftingSystem.MainEquip;
        _mainEquipSlot.Init();
        _mainEquipSlot.SetSlot(mainEquip.InventoryKey, mainEquip.Grade.ToString());
        //_mainEquipSlot.SetSlot(mainEquip.ItemID, mainEquip.Grade.ToString());
        _mainEquipSlot.AddListener(() => _craftingSystem.RemoveMainEquip());
    }

    // 재료 장비UI 세팅
    private void SetResourceEquipsUI()
    {
        if (CheckResourceEquips())
            return;

        int i = 0;
        for (; i < _craftingSystem.ResourceEquips.Length; i++)
        {
            if (_craftingSystem == null || _craftingSystem.ResourceEquips[i] == null)
            {
                _resourceCostSlots[i].Init();
                continue;
            }
            var resource = _craftingSystem.ResourceEquips[i];
            _resourceCostSlots[i].Init();
            _resourceCostSlots[i].SetSlot(resource.InventoryKey, resource.Grade.ToString());
            //_resourceCostSlots[i].SetSlot(resource.ItemID, resource.Grade.ToString());
            _resourceCostSlots[i].AddListener(() => _craftingSystem.RemoveResourceEquip(resource));
        }
        for(; i < _resourceCostSlots.Count; i++)
        {
            _resourceCostSlots[i].Init();
        }

    }

    private bool CheckResourceEquips()
    {
        bool isNull = _craftingSystem.ResourceEquips == null;
        if (isNull)
        {
            foreach(var slot in _resourceCostSlots)
            {
                slot.Init();
            }
        }

        return isNull;
    }
    #endregion

    #region 착용 장비창

    private void SetLoadoutUI()
    {
        int i = 0;
        foreach(var equip in _loadout.MyEquipments.Values)
        {
            var craftingInfo = new CraftingInfo(equip);
            _loadoutSlots[i].Init();
            _loadoutSlots[i].SetSlot(craftingInfo.InventoryKey);
            _loadoutSlots[i++].AddListener(() => _craftingSystem.SetCraftSlot(craftingInfo));
        }
    }

    #endregion

    #region 재료 장비창

    private void SetInventoryEquipUI()
    {
        int i = 0;
        foreach(var item in _inventory.Inventory)
        {
            var type = ItemDB.GetData(item.Key.ID).Type;
            if (type != ITEM_TYPE.Equipment)
                continue;

            _resourceEquipSlots[i].Init();
            _resourceEquipSlots[i].SetSlot(item.Key, item.Value);
            var craftInfo = new CraftingInfo(item.Key);
            _resourceEquipSlots[i++].AddListener(() => _craftingSystem.SetCraftSlot(craftInfo));
        }
        for(; i < _resourceEquipSlots.Count; i++)
        {
            _resourceEquipSlots[i].Init();
        }
    }



    #endregion
}
