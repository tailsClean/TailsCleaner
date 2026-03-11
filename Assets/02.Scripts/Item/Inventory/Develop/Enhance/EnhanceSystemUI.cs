using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnhanceSystem))]
public abstract class EnhanceSystemUI : MonoBehaviour
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

    protected Inventory _inventory;
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


    protected virtual void Start()
    {
        _inventory = _enhanceSystem.UsingInventory;
        _currency = _enhanceSystem.UsingCurrency;
        UpdateCurrentGold();
        UpdateResourceUI();
    }

    // 강화할 장비UI 갱신
    protected void UpdateEnhanceEquipmentUI(EnhancingInfo equipment)
    {
        ItemBaseSO resourceItem = ItemDB.GetItemData<ItemBaseSO>(equipment.EnhanceData.BluePrintID);
        ItemStack goldStack = _currency.GetGold();

        _enhanceEquipment.SetSlot(equipment.ItemID, equipment.EnhanceLevel);

        _resourceCostImage.sprite = resourceItem.ImageSprite;
        _resourceCostText.text = equipment.EnhanceData.CostBluePrint.ToString();
        _costGoldImage.sprite = goldStack.ItemData.ImageSprite;
        _costGoldText.text = equipment.EnhanceData.CostGold.ToString();
    }

    // 현재 골드량 갱신
    public abstract void UpdateCurrentGold();


    // 강화 재료 아이콘 전체 갱신
    public abstract void UpdateResourceUI();

}
