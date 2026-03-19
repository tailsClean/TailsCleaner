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


    [Header("선택 합성 장비")]
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

    #region 선택 장비창

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
        _mainEquipSlot.SetSlot(mainEquip.ItemID, mainEquip.Grad.ToString());
        _mainEquipSlot.AddListener(() => _craftingSystem.RemoveMainEquip(mainEquip));
    }

    // 재료 장비UI 세팅
    private void SetResourceEquipsUI()
    {
        if (CheckResourceEquips())
            return;

        for (int i = 0; i < _craftingSystem.ResourceEquips.Length; i++)
        {
            if (_craftingSystem == null || _craftingSystem.ResourceEquips[i] == null)
            {
                _resourceCostSlots[i].Init();
                continue;
            }
            var resource = _craftingSystem.ResourceEquips[i];
            _resourceCostSlots[i].Init();
            _resourceCostSlots[i].SetSlot(resource.ItemID, resource.Grad.ToString());
            _resourceCostSlots[i].AddListener(() => _craftingSystem.RemoveResourceEquip(resource));
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

        return _craftingSystem.ResourceEquips == null;
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
            _loadoutSlots[i].SetSlot(craftingInfo.ItemID, craftingInfo.Grad.ToString());
            _loadoutSlots[i++].AddListener(() => _craftingSystem.SetCraftSlot(craftingInfo));
        }
    }

    #endregion

    #region 재료 장비창

    private void SetInventoryEquipUI()
    {
        int i = 0;
        foreach(var item in _inventory.Inventory.Keys)
        {
            var type = ItemDB.GetData(item.ID).Type;
            //var type = ItemDB.GetData<ItemManageData>(item.ID).Type;
            if (type != ITEM_TYPE.Equipment)
                continue;

            _resourceEquipSlots[i].Init();
            _resourceEquipSlots[i].SetSlot(item.ID, item.Grade.ToString());
            var craftInfo = new CraftingInfo(item);
            _resourceEquipSlots[i++].AddListener(() => _craftingSystem.SetCraftSlot(craftInfo));
        }
        for(; i < _resourceEquipSlots.Count; i++)
        {
            _resourceEquipSlots[i].Init();
        }
    }



    #endregion
}
