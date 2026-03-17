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



    private void Start()
    {
        SetButton();
    }


    public void SetSlot(ItemInstance itemInstance) => _itemSlot.SetSlot(itemInstance.ID);

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
}