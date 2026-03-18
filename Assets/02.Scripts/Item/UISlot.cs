using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수정 필요
/// </summary>
public class UISlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountText;



    public ItemBase Item { get; private set; }

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
    public void SetSlot(int id, int? value = null)
    {
        var a = ItemDB.GetData<ItemDataBase>(id);
        if (a != null)
        {
            _image.sprite = a.SpriteImg;
            _amountText.text = value.ToString();
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
        }

        if (value == null)
            _amountText.text = string.Empty;
    }

    // 슬롯에 아이템(ID)과 벨류(갯수, 강화수치등)를 표시
    public void SetSlot(int id, string value)
    {
        var a = ItemDB.GetData<ItemDataBase>(id);
        if (a != null)
        {
            _image.sprite = a.SpriteImg;
            _amountText.text = value;
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
        }
    }

    //public void ShowSlot(ItemInstance item)
    //{
    //    var a = ItemDB.GetItemData<ItemBaseSO>(item.ID);
    //    if (a != null)
    //    {
    //        _image.sprite = a.ImageSprite;
    //        _amountText.text = item.Amount.ToString();
    //    }
    //    else
    //    {
    //        Debug.Log("값을 찾을 수 없다.");
    //        _image.sprite = _baseSprite;
    //        _amountText.text = string.Empty;
    //    }

    //    _button.onClick.AddListener( () => _onItemPopup.OnStartEvent(item) );
    //}


    public void AddListener(Action action)
    {
        if (_button == null)
            return;
            //{ Debug.LogWarning("UI슬롯에 버튼이 없어서 이벤트 등록X", this); return; }

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