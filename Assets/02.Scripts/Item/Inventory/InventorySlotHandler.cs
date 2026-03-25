using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlotHandler : UIGroup
{
    [Header("핸들러 데이터")]
    [SerializeField] protected List<UISlot> _slots;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Button _handlerButton;

    [Header("이벤트 채널")]
    [SerializeField] private ItemInstanceEventChannelSO _onItemPopup;

    private Dictionary<int, int> _inventory;
    private List<int> _itemOrder = new();                    // 무작위로 들고 있는 _inventory의 값들에 순서를 부여
    private Dictionary<int, UISlot> _slotByItemId = new();   // 특정 ID가 슬롯에 배치됐는지 조회용 딕셔너리
    private Image _buttonImg;
    private Sprite _baseSprite;

    protected List<ItemInstance> _items = new();


    private void Awake()
    {

        _itemOrder = new List<int>();
        _slotByItemId = new Dictionary<int, UISlot>();
        _buttonImg = _handlerButton.GetComponent<Image>();
        _baseSprite = _buttonImg.sprite;
    }

    private void OnEnable()
    {
        _buttonImg.sprite = _selectedSprite;
    }

    private void OnDisable()
    {
        _buttonImg.sprite = _baseSprite;
    }

    public virtual void UpdateInventory()
    {
        int i = 0;
        foreach(var item in _items)
        {
            if(item.Amount == ItemInstance.NoneStackAmount)
            {
                _slots[i].Init();
                _slots[i].AddListener(() => _onItemPopup.OnStartEvent(item));
                _slots[i++].SetSlot(item.ID);
            }
            else
            {
                _slots[i].Init();
                _slots[i].AddListener(() => _onItemPopup.OnStartEvent(item));
                _slots[i++].SetSlot(item, item.Amount);
            }
        }
        for(;i < _slots.Count; i++)
        {
            _slots[i].Init();
        }
    }






    public void HandlerInit()
    {
        _items.Clear();

        //_inventory = inventory;


    }
    // 인벤토리 아이템에 순서 부여
    //private void ItemIdOrderring()
    //{
    //    foreach (var id in _inventory.Keys)
    //    {
    //        _itemOrder.Add(id);
    //    }
    //}

    public void SetItem(ItemInstance item)
    {
        _items.Add(item);
    }


    #region 리팩토링하면 사용할 수도 있는 코드

    //// 인벤토리칸을 UI창에 업데이트 반영
    //public void UpdateInventory()
    //{
    //    for (int order = 0; order < _itemOrder.Count; order++)
    //    {
    //        SetOrderSlot(order);
    //    }
    //}
    //// 순서에 해당하는 슬롯에 아이템(ID) 배치
    //private void SetOrderSlot(int order)
    //{
    //    int id = _itemOrder[order];
    //    var slot = _slots[order];

    //    slot.SetSlot(id, _inventory[id]);
    //    _slotByItemId.TryAdd(id, slot);
    //}


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
    #endregion
}
