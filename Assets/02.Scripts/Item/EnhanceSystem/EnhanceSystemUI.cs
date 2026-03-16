using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnhanceSystem))]
public abstract class EnhanceSystemUI : UIGroup
{
    [Header("이벤트 채널")]
    [SerializeField] protected VoidEventChannelSO _onChangeInventory;
    [SerializeField] protected VoidEventChannelSO _onChangeGold;

    [Header("선택 강화 장비")]
    [SerializeField] protected UISlot _enhanceEquipment;
    [SerializeField] protected Image _resourceCostImage;
    [SerializeField] protected TextMeshProUGUI _resourceCostText;
    [SerializeField] protected Image _costGoldImage;
    [SerializeField] protected TextMeshProUGUI _costGoldText;

    protected EnhanceSystem _enhanceSystem;

    protected ItemInventory _inventory;
    protected Currency _currency;                         // 필요 금화를 읽어올 재화 가방



    protected virtual void Awake()
    {
        _enhanceSystem = GetComponent<EnhanceSystem>();
    }

    protected virtual void OnEnable()
    {
        if(_inventory != null)
            UpdateResourceUI();

        _enhanceSystem.OnSetEquipment += UpdateEnhanceEquipmentUI;
        _enhanceSystem.OnEnhance += UpdateEnhanceEquipmentUI;
        _onChangeInventory.AddListener(UpdateResourceUI);
        _onChangeGold.AddListener(UpdateCurrentGold);
    }

    protected virtual void OnDisable()
    {
        _enhanceSystem.OnSetEquipment -= UpdateEnhanceEquipmentUI;
        _enhanceSystem.OnEnhance -= UpdateEnhanceEquipmentUI;
        _onChangeInventory.RemoveListener(UpdateResourceUI);
        _onChangeGold.RemoveListener(UpdateCurrentGold);
    }


    protected override void Start()
    {
        base.Start();
        _inventory = _enhanceSystem.UsingInventory;
        _currency = _enhanceSystem.UsingCurrency;
        UpdateCurrentGold();
        UpdateResourceUI();
    }

    // 강화할 장비UI 갱신
    protected void UpdateEnhanceEquipmentUI(EnhancingInfo equipment)
    {
        ItemBaseSO resourceItem = ItemDB.GetItemData<ItemBaseSO>(equipment.EnhanceData.BluePrintID);
        ItemInstance gold = _currency.GetGold();
        var goldData = ItemDB.GetItemData<ItemBaseSO>(gold.ID);

        if (equipment.EnhanceData.IsMaxLevel)
        {
            _enhanceEquipment.SetSlot(equipment.ItemID, "Max");
            _resourceCostText.text = "Max";
            _costGoldText.text = "Max";
        }
        else
        {
            _enhanceEquipment.SetSlot(equipment.ItemID, equipment.EnhanceLevel);
            _resourceCostText.text = equipment.EnhanceData.CostBluePrint.ToString();
            _costGoldText.text = equipment.EnhanceData.CostGold.ToString();
        }

        _resourceCostImage.sprite = resourceItem.ImageSprite;
        _costGoldImage.sprite = goldData.ImageSprite;
    }

    // 현재 골드량 갱신
    public abstract void UpdateCurrentGold();


    // 강화 재료 아이콘 전체 갱신
    public abstract void UpdateResourceUI();

}
