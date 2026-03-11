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
        base.OnEnable();
        _onChangeInventory.AddListener(UpdateRelicSlots);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _onChangeInventory.RemoveListener(UpdateRelicSlots);
    }

    private void UpdateRelicSlots()
    {
        var relicInventory = _inventory.RelicStatus;
        int i = 0;
        foreach(var relic in relicInventory)
        {
            var slot = _relicSlots[i++];
            slot.Init();
            slot.SetSlot(relic.UniqueID);
            slot.AddListener(() => _enhanceSystem.SetRelic(relic.UniqueID, relic.EnhanceLevel));
        }
        for (; i < _relicSlots.Count; i++)
        {
            _relicSlots[i].Init();
        }
    }

    public override void UpdateResourceUI()
    {
        ItemStack resourceStack = _inventory.GetItemInfo(ItemID.RelicReinforceResource);
        _resourceImage.sprite = resourceStack.ItemData.ImageSprite;
        _resourceText.text = resourceStack.Amount.ToString();
    }

    public override void UpdateCurrentGold()
    {
        ItemStack goldStack = _currency.GetGold();
        _currentGoldImage.sprite = goldStack.ItemData.ImageSprite;
        _currentGoldText.text = goldStack.Amount.ToString();
    }
}
