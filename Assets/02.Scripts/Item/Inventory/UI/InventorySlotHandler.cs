using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlotHandler : UIGroup
{
    [SerializeField] private List<UISlot> _slots;
    [SerializeField] private Button _handlerButton;

    private Dictionary<int, int> _inventory;
    private List<int> _itemOrder = new();                           // 무작위로 들고 있는 _inventory의 값들에 순서를 부여
    private Dictionary<int, UISlot> _slotByItemId = new();   // 특정 ID가 슬롯에 배치됐는지 조회용 딕셔너리
    private Image _buttonImg;
    private Color _baseColor;




    private void Awake()
    {
        _itemOrder = new List<int>();
        _slotByItemId = new Dictionary<int, UISlot>();
        _buttonImg = _handlerButton.GetComponent<Image>();
        _baseColor = _buttonImg.color;
    }

    private void OnEnable()
    {
        _buttonImg.color = _handlerButton.colors.disabledColor;
    }

    private void OnDisable()
    {
        _buttonImg.color = _baseColor;
    }



    // 인벤토리칸을 UI창에 업데이트 반영
    public void UpdateInventory()
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
        var slot = _slots[order];

        slot.SetSlot(id, _inventory[id]);
        _slotByItemId.TryAdd(id, slot);
    }

    // 아이템을 슬롯에 추가
    public void AddItemFromSlot(int id) => _itemOrder.Add(id);

    // 제거 아이템을 아이템 순서리스트, 인벤토리 슬롯에서 지우기
    public void RemoveItemFromSlot(int id)
    {
        var slot = _slotByItemId[id];
        ReleaseSlot(slot);

        _itemOrder.Remove(id);
        _slotByItemId.Remove(id);

    }
    // 슬롯 자체를 초기화 후, 맨 뒤로 보내기
    private void ReleaseSlot(UISlot slot)
    {
        slot.Init();

        slot.transform.SetAsLastSibling();
        _slots.Remove(slot);
        _slots.Add(slot);
    }

    
    public void Init(Dictionary<int, int> inventory)
    {
        _inventory = inventory;
        ItemIdOrderring();
    }
    // 인벤토리 아이템에 순서 부여
    private void ItemIdOrderring()
    {
        foreach (var id in _inventory.Keys)
        {
            _itemOrder.Add(id);
        }
    }
}
