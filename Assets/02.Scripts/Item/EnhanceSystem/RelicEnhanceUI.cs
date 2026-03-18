using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicEnhanceUI : EnhanceSystemUI
{
    [Header("강화용 장비 선택창")]
    [SerializeField] private List<UISlot> _relicSlots;
    [SerializeField] private Image _resourceImage;
    [SerializeField] private TextMeshProUGUI _resourceText;
    [SerializeField] private Image _currentGoldImage;
    [SerializeField] private TextMeshProUGUI _currentGoldText;

    protected override void OnEnable()
    {
        if (_inventory != null)
            UpdateRelicSlots();

        base.OnEnable();
        _onChangeInventory.AddListener(UpdateRelicSlots);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _onChangeInventory.RemoveListener(UpdateRelicSlots);
    }

    protected override void Start()
    {
        base.Start();
        UpdateRelicSlots();
    }

    private void UpdateRelicSlots()
    {
        var relicInventory = _inventory.Inventory;
        int i = 0;
        foreach(var relic in relicInventory.Keys)
        {
            ITEM_TYPE type = ItemDB.GetData<RelicData>(relic.ID).Type;
            if (type != ITEM_TYPE.Relic)
                continue;

            var slot = _relicSlots[i++];
            slot.Init();
            slot.SetSlot(relic.ID);
            slot.AddListener(() => _enhanceSystem.SetRelic(relic.ID, relic.EnhanceLevel));
        }
        for (; i < _relicSlots.Count; i++)
        {
            _relicSlots[i].Init();
        }
    }

    public override void UpdateResourceUI()
    {
        _resourceImage.sprite = ItemDB.GetData<ItemDataBase>(ItemID.RelicReinforceResource).SpriteImg;

        if (!_inventory.TryGetStackItem(ItemID.RelicReinforceResource, out var item))
        {
            _resourceText.text = "0";
            return;
        }

        
        var itemData = ItemDB.GetData<ItemDataBase>(item.ID);
        _resourceImage.sprite = itemData.SpriteImg;

        if(item.Amount == ItemInstance.NoneStackAmount)
            _resourceText.text = item.Grade.ToString();
        else 
            _resourceText.text = item.Amount.ToString();
    }

    public override void UpdateCurrentGold()
    {
        ItemInstance gold = _currency.GetGold();
        var goldData = ItemDB.GetData<ItemManageData>(gold.ID);
        _currentGoldImage.sprite = goldData.SpriteImg;
        _currentGoldText.text = gold.Amount.ToString();
    }
}
