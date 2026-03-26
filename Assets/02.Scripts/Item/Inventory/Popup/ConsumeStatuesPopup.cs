
using UnityEngine;
using UnityEngine.UI;

public class ConsumeStatuesPopup : ItemPopup
{
    [SerializeField] private Button _cancleButton;
    [SerializeField] private Button _consumeButton;

    private ItemInventory _inventory;
    private ConsumeSystem _consumeSystem;


    protected override void Start()
    {
        base.Start();
        _inventory = ItemManager.Instance.Inventory;
        _consumeSystem = new ConsumeSystem(_inventory);
        Init();
    }


    // 팝업이 열릴 때, 해당 아이템 세팅 및 버튼에 메서드 추가
    public override void SetSlot(ItemInstance itemInstance)
    {
        base.SetSlot(itemInstance);
        SetConsumeButton();
    }


    // 아이템 사용 버튼에 메서드 추가
    private void SetConsumeButton()
    {
        if (_consumeButton == null)
            return;

        _consumeButton.onClick.RemoveAllListeners();
        _consumeButton.onClick.AddListener(ItemConsume);
    }

    // 아이템 사용 버튼 누르면 아이템 소모
    private void ItemConsume()
    {
        _consumeSystem.UseItem(_currentItem, 1, out bool isConsume);
        if(isConsume)
            _currentItem.SetAmount(_currentItem.Amount - 1);
    }

    private void Init()
    {
        SetCancleButton();
    }
    // 취소 버튼 메서드 추가
    private void SetCancleButton()
    {
        if (_cancleButton == null)
            return;

        _cancleButton.onClick.RemoveAllListeners();
        _cancleButton.onClick.AddListener( () => gameObject.SetActive(false) );
    }
}
