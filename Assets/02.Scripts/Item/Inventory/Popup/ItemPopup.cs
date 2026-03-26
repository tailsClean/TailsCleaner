using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ItemPopup : MonoBehaviour
{
    [field: SerializeField] public ITEM_TYPE ItemType { get; private set; }
    [SerializeField] private Button _background;
    [SerializeField] private UISlot _itemSlot;
    [SerializeField] private List<InventoryUIOpenSet> _openButton;

    [Header("판매용 버튼")]
    [SerializeField] private Button _sellingButton;
    [SerializeField] private SellingItemPopup _sellingPopup;

    [Header("유물 상태창 전용")]
    [SerializeField] private Button _releaseRelic;
    private PlayerLoadout _loadout;


    protected ItemInstance _currentItem;



    protected virtual void Start()
    {
        SetButton();
        OnSelling();
    }


    // 팝업이 열릴 때, 해당 아이템 세팅 및 버튼에 메서드 추가
    public virtual void SetSlot(ItemInstance itemInstance)
    {
        _currentItem = itemInstance;
        _itemSlot.SetSlot(itemInstance.ID);
        SetReleaseButton();
    }

    // 버튼 초기화
    private void SetButton()
    {
        // 백그라운드 클릭시, 팝업 닫히도록 메서드 추가
        var parent = _background.transform.parent;
        _background.onClick.AddListener( () => parent.gameObject.SetActive(false));

        // 버튼을 누르면 특정 UIGroup만 열리고 다른 창은 닫는 기능 버튼에 추가
        foreach(var button in _openButton)
        {
            button.PushButton.onClick.AddListener(() => OpenUI(button.OpenGroup));
        }
    }
    // 아이템 매니저에서 UIGroup 전체 닫히는 메서드 불러오기
    private void OpenUI(UI_GROUP group) => ItemManager.Instance.OpenUI(group);


    // 판매 기능 버튼에 추가
    private void OnSelling()
    {
        if(_sellingButton != null && _sellingPopup != null)
        {
            _sellingButton.onClick.AddListener(() => gameObject.SetActive(false));
            _sellingButton.onClick.AddListener(() => _sellingPopup.gameObject.SetActive(true));
            _sellingButton.onClick.AddListener(() => _sellingPopup.SetSlot(_currentItem));
        }
    }

    // 유물 팝업을 닫는 메서드를 버튼에 추가
    private void SetReleaseButton()
    {
        if (_loadout == null)
            _loadout = ItemManager.Instance.Loadout;

        if (_releaseRelic == null)
            return;

        _releaseRelic.onClick.RemoveAllListeners();
        _releaseRelic.onClick.AddListener(() => _loadout.RemoveRelic(_currentItem.ID, _currentItem.EnhanceLevel));
        _releaseRelic.onClick.AddListener(() => gameObject.SetActive(false));
    }

}