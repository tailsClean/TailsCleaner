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
    protected ItemCurrency _currency;                         // 필요 금화를 읽어올 재화 가방



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


        if (equipment.IsCurrentMaxLevel)
        {
            _enhanceEquipment.SetSlot(equipment.ItemID, "Max");
            _resourceCostText.text = "Max";
            _costGoldText.text = "Max";
        }
        else
        {
            var resourceItem = ItemDB.GetData(equipment.NextEnhanceData.BluePrintID);
            var goldData = ItemDB.GetData(ItemID.Gold);
            _enhanceEquipment.SetSlot(equipment.ItemID, equipment.CurrentEnhanceLevel);
            _resourceCostText.text = equipment.NextEnhanceData.CostBluePrint.ToString();
            _costGoldText.text = equipment.NextEnhanceData.CostGold.ToString();
            _resourceCostImage.sprite = resourceItem.SpriteImg;
            _costGoldImage.sprite = goldData.SpriteImg;
        }


    }

    // 현재 골드량 갱신
    public abstract void UpdateCurrentGold();


    // 강화 재료 아이콘 전체 갱신
    public abstract void UpdateResourceUI();

}
