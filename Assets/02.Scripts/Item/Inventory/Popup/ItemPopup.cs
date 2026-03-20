using System;
using System.Collections;
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


    public virtual void SetSlot(ItemInstance itemInstance)
    {
        _currentItem = itemInstance;
        _itemSlot.SetSlot(itemInstance.ID);
        SetReleaseButton();
    }


    private void SetButton()
    {
        var parent = _background.transform.parent;
        _background.onClick.AddListener( () => parent.gameObject.SetActive(false));

        foreach(var button in _openButton)
        {
            button.PushButton.onClick.AddListener(() => OpenUI(button.OpenGroup));
        }
    }
    private void OpenUI(UI_GROUP group) => ItemManager.Instance.OpenUI(group);


    private void OnSelling()
    {
        if(_sellingButton != null && _sellingPopup != null)
        {
            _sellingButton.onClick.AddListener(() => gameObject.SetActive(false));
            _sellingButton.onClick.AddListener(() => _sellingPopup.gameObject.SetActive(true));
            _sellingButton.onClick.AddListener(() => _sellingPopup.SetSlot(_currentItem));
        }
    }


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