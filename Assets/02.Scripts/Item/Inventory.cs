using System;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    [SerializeField] private Dictionary<int, int> _myInventory;         // Key: 아이템ID , Value: 소지갯수

    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;

    private void Awake()
    {
        _myInventory = new Dictionary<int, int>();
    }

    public void TestGain(int id)
    {
        GainItem(id);

    }
    public void TestUse(int id) => UseItem(id);

    // 아이템 획득시, 인벤토리 저장
    public void GainItem(int id, int amount = 1)
    {
        if (_myInventory.TryGetValue(id, out var item))
            _myInventory[id] += amount;

        else
        {
            _myInventory.Add(id, amount);
            OnAddItem?.Invoke(id);
        }
    }

    // 인벤토리의 아이템 사용
    public void UseItem(int id, int amount = 1)
    {
        if(!_myInventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return;
        }

        if(itemCount > amount)
            _myInventory[id] -= amount;

        else if(itemCount == amount)
        {
            _myInventory.Remove(id);
            OnRemoveItem?.Invoke(id);
        }

        else if(itemCount < amount)
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
    }



    //
    public TestItemIcon IconPrefab;
    public List<TestItemIcon> InventorySlots;

    private List<int> _itemOrder = new();
    private Dictionary<int, TestItemIcon> _slotByItemId = new();

    private void Start()
    {
        // 인벤토리 슬롯 생성
        for (int i = 0; i < 15; i++)
        {
            var a = Instantiate(IconPrefab, transform);
            InventorySlots.Add(a);
        }

        ItemIdOrderring();

        OnRemoveItem += RemoveItemFromSlot;
        OnAddItem += (id) => _itemOrder.Add(id);
    }

    private void Update()
    {
        UpdateInventory();
    }

    // 인벤토리 아이템에 순서 부여
    private void ItemIdOrderring()
    {
        foreach (var id in _myInventory.Keys)
        {
            _itemOrder.Add(id);
        }
    }

    // 인벤토리칸을 UI창에 업데이트 반영
    private void UpdateInventory()
    {
        for (int order = 0; order < _itemOrder.Count; order++)
        {
            SetOrderSlot(order);
        }
    }

    // 순서에 해당하는 슬롯에 아이템(ID) 배치
    private void SetOrderSlot(int order)
    {
        int id = _itemOrder[order];
        var slot = InventorySlots[order];

        slot.SetIcon(id, _myInventory[id]);
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

    private void ReleaseSlot(TestItemIcon slot)
    {
        slot.Init();

        // 슬롯 자체를 맨 뒤로 보내는 것
        slot.transform.SetAsLastSibling();
        InventorySlots.Remove(slot);
        InventorySlots.Add(slot);
    }
    //
}