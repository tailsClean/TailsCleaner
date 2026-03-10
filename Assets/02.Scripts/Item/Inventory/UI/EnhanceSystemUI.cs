using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnhanceSystem))]
public class EnhanceSystemUI : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;
    [SerializeField] private VoidEventChannelSO _onChangeGold;

    [Header("선택 강화 장비")]
    [SerializeField] private UISlot _enhanceEquipment;
    [SerializeField] private Image _resourceCostImage;
    [SerializeField] private TextMeshProUGUI _resourceCostText;
    [SerializeField] private Image _costGoldImage;
    [SerializeField] private TextMeshProUGUI _costGoldText;

    [Header("강화용 장비 선택창")]
    [SerializeField] private List<UISlot> _loadoutSlots;
    [SerializeField] private List<UISlot> _resourceSlots;
    [SerializeField] private Image _currentGoldImage;
    [SerializeField] private TextMeshProUGUI _currentGoldText;

    private EnhanceSystem _enhanceSystem;
    private PlayerLoadout _playerLoadout;               // 장착 장비 확인을 위함
    private Inventory _inventory;
    private Currency _currency;                         // 필요 금화를 읽어올 재화 가방



    private void Awake()
    {
        _enhanceSystem = GetComponent<EnhanceSystem>();

    }

    private void OnEnable()
    {
        if(_playerLoadout != null)
            UpdateLoadout();

        if(_inventory != null)
            UpdateResourceUI();

        _enhanceSystem.OnSetEquipment += UpdateEnhanceEquipmentUI;
        _enhanceSystem.OnEnhace += UpdateEnhanceEquipmentUI;
        _onChangeInventory.AddListener(UpdateResourceUI);
        _onChangeGold.AddListener(UpdateCurrentGold);
    }

    private void OnDisable()
    {
        InitLoadout();
        _enhanceSystem.OnSetEquipment -= UpdateEnhanceEquipmentUI;
        _enhanceSystem.OnEnhace += UpdateEnhanceEquipmentUI;
        _onChangeInventory.RemoveListener(UpdateResourceUI);
        _onChangeGold.RemoveListener(UpdateCurrentGold);
    }


    private void Start()
    {

        _inventory = _enhanceSystem.UsingInventory;
        _currency = _enhanceSystem.UsingCurrency;
        _playerLoadout = ItemManager.Instance.Loadout;
        UpdateLoadout();
        UpdateResourceUI();
        UpdateCurrentGold();
    }

    // 강화할 장비UI 갱신
    private void UpdateEnhanceEquipmentUI(EquipmentBase equipment)
    {
        ItemBaseSO resourceItem = ItemDB.GetItemData<ItemBaseSO>(equipment.EnhanceData.BluePrintID);
        ItemStack goldStack = _currency.GetGold();

        _enhanceEquipment.SetSlot(equipment.Data.UniqueID, equipment.EnhanceLevel);

        _resourceCostImage.sprite = resourceItem.ImageSprite;
        _resourceCostText.text = equipment.EnhanceData.CostBluePrint.ToString();
        _costGoldImage.sprite = goldStack.ItemData.ImageSprite;
        _costGoldText.text = equipment.EnhanceData.CostGold.ToString();
    }

    private void UpdateLoadout()
    {
        int i = 0;
        foreach (var equipment in _playerLoadout.MyEquipments.Values)
        {
            var slot = _loadoutSlots[i++];
            slot.SetSlot(equipment.Data.UniqueID);
            slot.AddListner(() => _enhanceSystem.SetEquipment(equipment.Data.EquipmentPart));
        }
    }

    private void InitLoadout()
    {
        foreach(var slot in _loadoutSlots)
        {
            slot.Init();
        }
    }


    // 강화 재료 아이콘 전체 갱신
    private void UpdateResourceUI()
    {
        UpdateResourceIcon(0, ItemID.WeaponReinforceResource);
        UpdateResourceIcon(1, ItemID.HatReinforceResource);
        UpdateResourceIcon(2, ItemID.CloakReinforceResource);
        UpdateResourceIcon(3, ItemID.ShoseReinforceResource);
    }
    // 강화 재료 아이콘 갱신
    private void UpdateResourceIcon(int index, int id)
    {
        var dict = _inventory.ReinforceResourceInventory;
        if(dict.TryGetValue(id, out var amount))
        {
            _resourceSlots[index].SetSlot(id, amount);
            return;
        }
        _resourceSlots[index].SetSlot(id, 0);
    }

    // 현재 골드량 갱신
    private void UpdateCurrentGold()
    {
        ItemStack goldStack = _currency.GetGold();
        _currentGoldImage.sprite = goldStack.ItemData.ImageSprite;
        _currentGoldText.text = goldStack.Amount.ToString();
    }
}
