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
    [SerializeField] protected UISlot _mainEquipSlot;
    [SerializeField] protected List<Image> _resourceCostImage;
    [SerializeField] protected List<TextMeshProUGUI> _resourceCostText;

    private Sprite _baseSprite;

    private void Awake()
    {
        _craftingSystem = GetComponent<CraftingSystem>();
        _baseSprite = _resourceCostImage[0].sprite;
    }

    private void Start()
    {
        _inventory = _craftingSystem.UsingInventory;
        _currency = _craftingSystem.UsingCurrency;
    }

    private void Update()
    {
        
    }

    private void SetMainEquipUI()
    {
        if(_craftingSystem.MainEquip == null)
        {
            _mainEquipSlot.Init();
            return;
        }


    }
}
