using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemInventoryUI : UIGroup
{
    [SerializeField] private ItemInventory _inventory;

    [Header("아이템 팝업UI")]
    [SerializeField] private List<ItemPopup> _itemPopupUI;
    [SerializeField] private UISlot _popupUISlot;

    [Header("인벤토리 패널리스트")]
    [SerializeField] private List<Button> _InventorySelectButtons;
    [SerializeField] private List<InventorySlotHandler> _slotHandlerList;

    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;
    [SerializeField] private ItemInstanceEventChannelSO _onItemPopUp;

    private Dictionary<UI_GROUP, InventorySlotHandler> _uiGroupDict;
    private InventorySlotHandler _currentShowUI;



    private void Awake()
    {
        InventoryUIInit();
    }

    protected override void Start()
    {
        base.Start();
        foreach(var popup in _itemPopupUI)
        {
            popup.gameObject.SetActive(false);
        }
        
        SetSlotHandler();
        SetSelectButtons();
    }

    private void OnEnable()
    {
        _onChangeInventory.AddListener(SetSlotHandler);
        _onItemPopUp.AddListener(ShowPopup);
    }

    private void OnDisable()
    {
        _onChangeInventory.RemoveListener(SetSlotHandler);
        _onItemPopUp.RemoveListener(ShowPopup);
    }

    private void Update()
    {
        _currentShowUI.UpdateInventory();
    }


    // UI창 선택 버튼에 메서드 등록
    private void SetSelectButtons()
    {
        _InventorySelectButtons[0].onClick.AddListener(() => ShowUIGroup(UI_GROUP.EquipmentPanel));
        _InventorySelectButtons[1].onClick.AddListener(() => ShowUIGroup(UI_GROUP.RelicPanel));
        _InventorySelectButtons[2].onClick.AddListener(() => ShowUIGroup(UI_GROUP.ReinforceResourcePanel));
        _InventorySelectButtons[3].onClick.AddListener(() => ShowUIGroup(UI_GROUP.SpendablePanel));
    }


    public void ShowEquipUI()
    {
        gameObject.SetActive(true);
        ShowUIGroup(UI_GROUP.EquipmentPanel);
    }

    public void ShowRelicUI()
    {
        gameObject.SetActive(true);
        ShowUIGroup(UI_GROUP.RelicPanel);
    }

    // 특정 인벤토리 창 보여주는 메서드
    public void ShowUIGroup(UI_GROUP group)
    {
        foreach (var slotHandler in _uiGroupDict)
        {
            if (slotHandler.Key == group)
            {
                _currentShowUI.gameObject.SetActive(false);
                slotHandler.Value.gameObject.SetActive(true);
                _currentShowUI = slotHandler.Value;
            }
        }
    }


    // 슬롯핸들러에 특정 인벤토리(장비, 유물, 강화재료, 소모품)를 주입
    public void SetSlotHandler()
    {
        foreach(var slot in _slotHandlerList)
        {
            slot.HandlerInit();
        }
        foreach(var itemInstance in _inventory.Inventory)
        {
            ItemInstance itemKey = itemInstance.Key;
            itemKey.SetAmount(itemInstance.Value);

            if (!ItemDB.TryGetData<ItemDataBase>(itemInstance.Key.ID, out var itemData))
                continue;
            
            ITEM_TYPE type = itemData.Type;
            switch (type)
            {
                case ITEM_TYPE.Equipment:
                    SetItem(UI_GROUP.EquipmentPanel, itemKey);
                    break;

                case ITEM_TYPE.Relic:
                    SetItem(UI_GROUP.RelicPanel, itemKey);
                    break;

                case ITEM_TYPE.Reinforcement:
                    SetItem(UI_GROUP.ReinforceResourcePanel, itemKey);
                    break;

                case ITEM_TYPE.Consume:
                    SetItem(UI_GROUP.SpendablePanel, itemKey);
                    break;
            }
        }
    }
    private void SetItem(UI_GROUP group, ItemInstance itemInstance) => _uiGroupDict[group].SetItem(itemInstance);


    public void InventoryUIInit()
    {
        _uiGroupDict = new Dictionary<UI_GROUP, InventorySlotHandler>();

        // 슬롯핸들러 도감 등록
        foreach (var slotHandler in _slotHandlerList)
        {
            slotHandler.gameObject.SetActive(false);
            _uiGroupDict.Add(slotHandler.Group, slotHandler);
        }

        _currentShowUI = _slotHandlerList[0];
        _currentShowUI.gameObject.SetActive(true);
    }

    #region 아이템 팜업
    private void ShowPopup(ItemInstance item)
    {
        foreach(var popup in _itemPopupUI)
        {
            if(popup.ItemType == item.ItemType)
            {
                popup.gameObject.SetActive(true);
                popup.SetSlot(item);
                break;
            }
            else
                popup.gameObject.SetActive(false);
        }
    }


    #endregion
}
