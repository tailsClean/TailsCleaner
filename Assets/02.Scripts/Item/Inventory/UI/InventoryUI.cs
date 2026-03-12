using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Button _defaultButton;
    [SerializeField] private List<InventorySlotHandler> _slotHandlerList;

    private Dictionary<UI_GROUP, InventorySlotHandler> _uiGroupDict;
    private InventorySlotHandler _currentShowUI;



    private void Awake()
    {
        Init();
        //_defaultButton?.Select();
    }

    private void Start()
    {
        foreach(var slotHandler in _slotHandlerList)
        {
            InitSlotHandler(slotHandler);
        }
    }

    private void Update()
    {
        _currentShowUI.UpdateInventory();
    }



    //
    public void TestShow(int i) => ShowUIGroup((UI_GROUP)i);
    //


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

        SetInventoryEvent();
    }


    // 슬롯핸들러에 특정 인벤토리(장비, 유물, 강화재료, 소모품)를 주입
    public void InitSlotHandler(InventorySlotHandler slotHandler)
    {
        switch(slotHandler.Group)
        {
            case UI_GROUP.Equipment:
                SetInventory(UI_GROUP.Equipment, _inventory.EquipInventory);
                break;

            case UI_GROUP.Relic:
                SetInventory(UI_GROUP.Relic, _inventory.RelicInventory);
                break;

            case UI_GROUP.ReinforceResource:
                SetInventory(UI_GROUP.ReinforceResource, _inventory.ReinforceResourceInventory);
                break;

            case UI_GROUP.Spendable:
                SetInventory(UI_GROUP.Spendable, _inventory.ConsumeInventory);
                break;
        }
    }
    // 해당 그룹에 인벤토리 주입
    private void SetInventory(UI_GROUP group, Dictionary<int, int> inventory) => _uiGroupDict[group].Init(inventory);


    public void Init()
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
        SetInventoryEvent();
    }

    // 인벤토리 추가, 삭제 시에 이벤트를 현재 출력 중인 슬롯 핸들러가 메서드 구독
    private void SetInventoryEvent()
    {
        _inventory.InitEvent();
        _inventory.OnAddItem += _currentShowUI.AddItemFromSlot;
        _inventory.OnRemoveItem += _currentShowUI.RemoveItemFromSlot;
    }
}
