using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentEnhanceUI : EnhanceSystemUI
{
    [Header("강화용 장비 선택창")]
    [SerializeField] private List<UISlot> _loadoutSlots;
    [SerializeField] private List<UISlot> _resourceSlots;
    [SerializeField] private Image _currentGoldImage;
    [SerializeField] private TextMeshProUGUI _currentGoldText;

    private PlayerLoadout _playerLoadout;               // 장착 장비 확인을 위함

    protected override void OnEnable()
    {
        if(_playerLoadout != null)
            UpdateLoadout();

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        InitLoadoutSlot();
    }


    protected override void Start()
    {
        _playerLoadout = ItemManager.Instance.Loadout;
        base.Start();
        UpdateLoadout();
    }


    // 착용 장비창UI 갱신
    public void UpdateLoadout()
    {
        int i = 0;
        foreach (var equipment in _playerLoadout.MyEquipments.Values)
        {
            var slot = _loadoutSlots[i++];
            slot.SetSlot(equipment.Data.Equipmnet.id);
            slot.AddListener(() => _enhanceSystem.SetEquipment(equipment.Data.Equipmnet.part));
        }
    }

    // 착용 장비창UI 슬롯 초기화
    private void InitLoadoutSlot()
    {
        foreach (var slot in _loadoutSlots)
        {
            slot.Init();
        }
    }


    // 강화 재료 아이콘 전체 갱신
    public override void UpdateResourceUI()
    {
        UpdateResourceIcon(0, ItemID.WeaponReinforceResource);
        UpdateResourceIcon(1, ItemID.HatReinforceResource);
        UpdateResourceIcon(2, ItemID.CloakReinforceResource);
        UpdateResourceIcon(3, ItemID.ShoseReinforceResource);
    }
    // 강화 재료 아이콘 갱신
    private void UpdateResourceIcon(int index, int id)
    {
        if(_inventory.TryGetStackItem(id, out var item))
        {
            _resourceSlots[index].SetSlot(id, item.Amount);
            return;
        }

        _resourceSlots[index].SetSlot(id, 0);
    }


    // 현재 골드량 갱신
    public override void UpdateCurrentGold()
    {
        ItemInstance gold = _currency.GetGold();
        var goldData = ItemDB.GetData<ItemManageData>(gold.ID);
        _currentGoldImage.sprite = goldData.SpriteImg;
        _currentGoldText.text = gold.Amount.ToString();
    }
}
