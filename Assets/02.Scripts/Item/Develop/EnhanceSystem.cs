using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceSystem : MonoBehaviour
{
    [SerializeField] private Currency _currency;                // 필요 금화를 읽어올 재화 가방
    [SerializeField] private Inventory _inventory;

    private PlayerLoadout _playerLoadout;

    private IEnhancement _enhanceItem;
    private Sprite _enhanceItemSprite;
    private int _bluePrintID;
    private int _bluePrintCost;
    private StackableItem _gold;
    private bool _isEnhancable;






    //
    [Header("UI관련")]
    [SerializeField] private Image _enhanceItemImage;
    [SerializeField] private TextMeshProUGUI _enhanceLevelText;
    [SerializeField] private Image _reinforceResourceImage;
    [SerializeField] private TextMeshProUGUI _costBluePrintText;
    [SerializeField] private TextMeshProUGUI _costGoldText;
    [SerializeField] private Image _goldImage;
    [SerializeField] private TextMeshProUGUI _goldText;

    public List<InventorySlot> _loadoutSlots;
    public List<InventorySlot> _resourceSlots;
    //


    private void Start()
    {
        _playerLoadout = ItemManager.Instance.Loadout;
        
        //
        int i = 0;
        foreach(var equipment in _playerLoadout.MyEquipments.Values)
        {
            var slot = _loadoutSlots[i++];
            slot.SetSlot(equipment.Data.UniqueID, 1, () => SetEquipment(equipment.Data.EquipmentPart));
        }
        //
    }

    //
    private void Update()
    {
        _goldText.text = "재화: " + _currency.GoldAmount.ToString();

        if (_enhanceItem != null)
        {
            var gold = ItemDB.GetItemSO<StackableItemSO>(9);

            _enhanceItemImage.sprite = _enhanceItemSprite;
            _reinforceResourceImage.sprite = _inventory.GetItem(_bluePrintID).ImageSprite;
            _goldImage.sprite = gold.ImageSprite;
            _costBluePrintText.text = _enhanceItem.EnhanceData.CostBluePrint.ToString();
            _costGoldText.text = _enhanceItem.EnhanceData.CostGold.ToString();
            _enhanceLevelText.text = "+" + _enhanceItem.EnhanceLevel.ToString();
        }
        else
        {
            _costBluePrintText.text = "";
            _costGoldText.text = "";
            _enhanceLevelText.text = "";
        }
        ResourceImage(0, ItemID.WeaponReinforceResource);
        ResourceImage(1, ItemID.HatReinforceResource);
        ResourceImage(2, ItemID.CloakReinforceResource);
        ResourceImage(3, ItemID.ShoseReinforceResource);
    }


    //public void RemoveSlots(List<InventorySlot> slots)
    //{
    //    foreach(var slot in slots)
    //    {
    //        Destroy(slot.gameObject);
    //    }
    //}
    private void ResourceImage(int index, int id)
    {
        var dict = _inventory.ReinforceResourceInventory;
        if(dict.TryGetValue(id, out var amount))
        {
            _resourceSlots[index].SetSlot(id, amount);
            return;
        }
        _resourceSlots[index].SetSlot(id, 0);
    }

    //

    // 강화할 아이템 세팅
    public void SetEquipment(EQUIP_PARTS part)
    {
        EquipmentBase equipment = _playerLoadout.MyEquipments[part];
        _enhanceItem = equipment;
        _enhanceItemSprite = equipment.Data.ImageSprite;
        _bluePrintID = equipment.EnhanceData.BluePrintID;
        _bluePrintCost = equipment.EnhanceData.CostBluePrint;
    }


    public void OnEnhance()
    {
        _isEnhancable = true;
        CheckMaxLevel();
        CheckGoldCost();
        CheckResourceCost();

        if (_isEnhancable)
        {
            _enhanceItem.OnEnhance();

            _currency.UseGold(_enhanceItem.EnhanceData.CostGold);
            _inventory.UseItem(ITEM_TYPE.Reinforcement, _bluePrintID, _bluePrintCost);
            Debug.Log("강화성공!");
        }
    }


    // 최대 강화수치인지 확인
    private void CheckMaxLevel()
    {
        if (!_isEnhancable)
            return;

        _isEnhancable = !_enhanceItem.EnhanceData.IsMaxLevel;

        if (!_isEnhancable)
            Debug.Log("강화수치가 최대입니다.");
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if (!_isEnhancable)
            return;

        int cost = _enhanceItem.EnhanceData.CostGold;

        if (cost < 0)
        { Debug.LogError("골드 코스트가 음수입니다!"); return; }

        _isEnhancable = _currency.TryUseGold(cost);

        if (!_isEnhancable)
            Debug.Log("골드가 부족합니다.");
    }


    // 재료 재화와 코스트 비교
    private void CheckResourceCost()
    {
        if (!_isEnhancable)
            return;

        if (_bluePrintCost < 0)
        { Debug.LogError("재료 코스트가 음수입니다!"); return; }

        _isEnhancable = _inventory.TryUseItem(ITEM_TYPE.Reinforcement, _bluePrintID, _bluePrintCost);

        if (!_isEnhancable)
            Debug.Log("강화 재료가 부족합니다.");
    }
}
