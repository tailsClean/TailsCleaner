using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(CraftingSystem))]
public class CraftingSystemUI : MonoBehaviour
{
    private CraftingSystem _craftingSystem;
    private Inventory _inventory;
    private Currency _currency;


    [Header("선택 합성 장비")]
    [SerializeField] private UISlot _mainEquipSlot;
    [SerializeField] private List<UISlot> _resourceCostSlots;

    [Header("장비 선택리스트")]
    //[SerializeField] private List<UISlot> _loadoutSlots;
    [SerializeField] private List<UISlot> _resourceEquipSlots;



    private void Awake()
    {
        _craftingSystem = GetComponent<CraftingSystem>();
    }

    private void Start()
    {
        _inventory = _craftingSystem.UsingInventory;
        _currency = _craftingSystem.UsingCurrency;
    }

    private void Update()
    {
        SetMainEquipUI();
        SetResourceEquipsUI();

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

    #region 재료 장비창

    private void SetInventoryEquipUI()
    {
        int i = 0;
        foreach(var equip in _inventory.EquipStates)
        {
            _resourceEquipSlots[i].Init();
            _resourceEquipSlots[i].SetSlot(equip.UniqueID, equip.Grade.ToString());
            var craftInfo = new CraftingInfo(equip);
            _resourceEquipSlots[i++].AddListener(() => _craftingSystem.SetCraftSlot(craftInfo));
        }
        for(; i < _resourceEquipSlots.Count; i++)
        {
            _resourceEquipSlots[i].Init();
        }
    }



    #endregion
}
