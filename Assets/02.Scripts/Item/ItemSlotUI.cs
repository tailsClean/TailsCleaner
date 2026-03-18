using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수정 필요
/// </summary>
public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountText;

    [Header("이벤트 채널")]
    [SerializeField] private ItemInstanceEventChannelSO _onItemPopup;

    private ItemInstance _itemData;

    private Image _image;
    private Button _button;
    private Sprite _baseSprite;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _baseSprite = _image.sprite;
    }

    // 슬롯에 아이템(ID)과 벨류(갯수, 강화수치등)를 표시
    public void SetSlot(ItemInstance item)
    {
        _itemData = item;
        ShowSlot(item);
    }

    public void ShowSlot(ItemInstance item)
    {
        var a = ItemDB.GetData<ItemDataBase>(item.ID);
        if (a != null)
        {
            _image.sprite = a.SpriteImg;
            _amountText.text = item.Amount.ToString();
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
        }
    }


    public void AddListener(Action action)
    {
        if (_button == null)
        { Debug.LogWarning("UI슬롯에 버튼이 없어서 이벤트 등록X", this); return; }

        _button.onClick.AddListener(() => action());
    }

    public void Init()
    {
        _image.sprite = _baseSprite;
        _amountText.text = string.Empty;

        if(_button != null)
            _button.onClick.RemoveAllListeners();
    }
}