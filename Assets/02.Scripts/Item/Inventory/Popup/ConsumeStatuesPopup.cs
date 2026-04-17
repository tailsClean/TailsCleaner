using UnityEngine;
using UnityEngine.UI;
using static UISlotAddedText;

public class ConsumeStatuesPopup : ItemPopupBase
{
    [Header("=============== 소비템 팝업 전용 ==================================")]
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
        SlotNameAndDesc();
        SetConsumeButton();
    }

    #region 내부 메서드

    // 아이템 이름과 설명 출력
    private void SlotNameAndDesc()
    {
        if (!ItemDB.TryGetData<ItemManageData>(_currentItem.ID, out var itemData))
        { Debug.LogWarning($"{_currentItem.Name}({_currentItem.ID})의 정보를 읽어올 수 없습니다.", this); return; }

        string nameText = itemData.Name;
        string descText = GetItemDesc(itemData);

        _itemSlot.SetAddedText(TEXT_TYPE.Name, nameText);
        _itemSlot.SetAddedText(TEXT_TYPE.Desc, descText);
    }

    // 아이템의 설명
    private string GetItemDesc(ItemManageData item)
    {

        string itemScrptKey = item.ManageData.item_script_key;
        StringSO stringDataSO = DataManager.Instance.GetSOData<StringSO>();
        var stringData = stringDataSO.GetById(itemScrptKey);

        return stringData != null ? stringData.kr : string.Empty;
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

        CheckAllUsing();
    }

    // 아이템 모두 소진 시, 팝업 종료
    private void CheckAllUsing()
    {
        if(!_inventory.TryGetStackItem(_currentItem.ID, out var item))
            gameObject.SetActive(false);
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

    #endregion
}
