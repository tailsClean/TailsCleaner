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
        var a = ItemDB.GetData(id);
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
        var a = ItemDB.GetData(id);
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

    public void SetSlot(ItemInstance item, TEXT_TYPE textType = TEXT_TYPE.Amount)
    {
        var itemData = ItemDB.GetData(item.ID);
        if (itemData == null)
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
            return;
        }
        if (itemData.TryGetData<DefaultEquipData>(out var equipData))
        {
            _image.sprite = equipData.GetEquipSprite(item.Grade);
            return;
        }

        //if(itemData.TryGetData<MaterialEquipData>(out var materData))
            //_image.sprite = materData


        _image.sprite = itemData.SpriteImg;

        if (_amountText == null)
            return;

        switch(textType)
        {
            case TEXT_TYPE.Amount:
                _amountText.text = item.Amount.ToString();
                break;
            case TEXT_TYPE.Name:
                _amountText.text = item.Name;
                break;
            case TEXT_TYPE.None:
                _amountText.text = string.Empty;
                break;
        }
    }

    public void SetSlot(ItemInstance item, string value)
    {
        var itemData = ItemDB.GetData(item.ID);
        if (itemData == null)
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
            return;
        }
        if (itemData.TryGetData<DefaultEquipData>(out var equipData))
        {
            _image.sprite = equipData.GetEquipSprite(item.Grade);
            return;
        }

        _image.sprite = itemData.SpriteImg;

        if (_amountText == null)
            return;

        _amountText.text = value;
    }


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

    public enum TEXT_TYPE
    {
        Amount, Name, None
    }
}