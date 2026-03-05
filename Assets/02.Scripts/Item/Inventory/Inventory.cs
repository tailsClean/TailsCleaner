using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour
{
    // Key: 아이템ID , Value: 소지갯수
    private Dictionary<int, int> _EquipInventory;
    private Dictionary<int, int> _relicInventory;
    private Dictionary<int, int> _reinforceMaterialInventory;
    private Dictionary<int, int> _spendableInventory;

    private int _moneyAmount;

    public int MoneyID { get; private set; } = 15;
    public int MoneyAmount
    {
        get
        {
            if (_moneyAmount < 0)
                Debug.LogError("현재 Money가 음수입니다.");
            return Mathf.Max(_moneyAmount, 0);
        }

    }

    public Dictionary<int, int> MyInventory => _EquipInventory;
    public Dictionary<int, int> SpendableInventory => _spendableInventory;


    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;


    private void Awake()
    {
        _EquipInventory = new Dictionary<int, int>();
        _spendableInventory = new Dictionary<int, int>();
    }



    //
    public void TestGain(int id)
    {
        GainItem(_EquipInventory, id);

    }
    public void TestUse( int id) => UseItem(_EquipInventory, id);
    //



    // 아이템 획득시, 인벤토리 저장
    public void GainItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (inventory.TryGetValue(id, out var item))
            inventory[id] += amount;

        else
        {
            inventory.Add(id, amount);
            OnAddItem?.Invoke(id);
        }
    }

    // 인벤토리의 아이템 사용
    public void UseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if(!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return;
        }

        if(itemCount > amount)
            inventory[id] -= amount;

        else if(itemCount == amount)
        {
            inventory.Remove(id);
            OnRemoveItem?.Invoke(id);
        }

        else if(itemCount < amount)
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
    }

    // 머니 관련 메서드
    public void GainMoney(int amount) => _moneyAmount += amount;
    public bool IsUseMoney(int amount)
    {
        if (amount < 0)
            return false;

        if(_moneyAmount < amount)
        {
            Debug.LogWarning("사용금액이 현재 금액을 초과합니다.");
            return false;
        }

        _moneyAmount -= amount;
        return true;
    }

    #region Money UI
    //
    public Image MoneyImage;
    public Text MoneyText;

    public void SetMoney()
    {
        MoneyImage.sprite = ItemDB.GetItem<StackableItem>(MoneyID).GetSprite();
        MoneyText.text = MoneyAmount.ToString();
    }

    //
    #endregion









    #region UI출력 방식
    //
    public InventorySlot InventorySlotPrefab;
    public List<InventorySlot> InventorySlots;                       // 아이템 슬롯UI요소

    private List<int> _itemOrder = new();                           // 무작위로 들고 있는 _myInventory의 값들에 순서를 부여
    private Dictionary<int, InventorySlot> _slotByItemId = new();    // 특정 ID가 슬롯에 배치됐는지 조회용 딕셔너리

    private void Start()
    {
        // 인벤토리 슬롯 생성
        for (int i = 0; i < 15; i++)
        {
            var a = Instantiate(InventorySlotPrefab, transform);
            InventorySlots.Add(a);
        }

        ItemIdOrderring(_EquipInventory);

        OnRemoveItem += RemoveItemFromSlot;
        OnAddItem += (id) => _itemOrder.Add(id);

        //MoneyImage.sprite = 
    }

    private void Update()
    {
        UpdateInventory(_EquipInventory);

        SetMoney();
    }

    // 인벤토리 아이템에 순서 부여
    private void ItemIdOrderring(Dictionary<int, int> inventory)
    {
        foreach (var id in inventory.Keys)
        {
            _itemOrder.Add(id);
        }
    }

    // 인벤토리칸을 UI창에 업데이트 반영
    private void UpdateInventory(Dictionary<int, int> inventory)
    {
        for (int order = 0; order < _itemOrder.Count; order++)
        {
            SetOrderSlot(inventory, order);
        }
    }

    // 순서에 해당하는 슬롯에 아이템(ID) 배치
    private void SetOrderSlot(Dictionary<int, int> inventory, int order)
    {
        int id = _itemOrder[order];
        var slot = InventorySlots[order];

        slot.SetIcon(id, inventory[id]);
        _slotByItemId.TryAdd(id, slot);
    }

    // 제거 아이템을 아이템 순서리스트, 인벤토리 슬롯에서 지우기
    public void RemoveItemFromSlot(int id)
    {
        // 슬롯을 초기화 후 맨 나중 슬롯으로 밀어버리기
        var slot = _slotByItemId[id];
        ReleaseSlot(slot);


        _itemOrder.Remove(id);
        _slotByItemId.Remove(id);

    }

    private void ReleaseSlot(InventorySlot slot)
    {
        slot.Init();

        // 슬롯 자체를 맨 뒤로 보내는 것
        slot.transform.SetAsLastSibling();
        InventorySlots.Remove(slot);
        InventorySlots.Add(slot);
    }
    //
    #endregion
}