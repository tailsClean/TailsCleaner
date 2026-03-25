using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class UISlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountText;

    private Image _image;
    private Button _button;
    private Sprite _baseSprite;



    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _baseSprite = _image.sprite;
        InitializedSlot();
    }



    // 슬롯에 아이템(ID)과 벨류(갯수, 강화수치등)를 표시
    public void SetSlot(int id, int value)
    {
        ShowSprite(id, out var item);
        ShowText(value);
    }

    // 슬롯에 아이템(ID)과 벨류(갯수, 강화수치등)를 표시
    public void SetSlot(int id, string value = null)
    {
        ShowSprite(id, out var item);
        ShowText(value);
    }

    // 슬롯에 특정 아이템을 넣으면 자동으로 아이템의 정보를 UI로 출력
    public void SetSlot(ItemInstance item, string value = null)
    {
        ShowSprite(item.ID, out var itemData);
        if (itemData == null)
        {
            InitializedSlot();
            return;
        }

        if (itemData.TryGetData<DefaultEquipData>(out var equipData))
            _image.sprite = equipData.GetEquipSprite(item.Grade);

        if (_amountText == null)
            return;

        if (value != null)
            ShowText(value);
        else
            ShowText();
            
    }

    // int를 통해서도 값을 출력할 수 있도록 오버로딩
    public void SetSlot(ItemInstance item, int value)
    {
        string text = value.ToString();
        SetSlot(item.ID, text);
    }


    // 버튼에 메서드 등록하기
    public void AddListener(Action action)
    {
        if (_button == null)
            return;
        //{ Debug.LogWarning("UI슬롯에 버튼이 없어서 이벤트 등록X", this); return; }

        _button.onClick.AddListener(() => action());
    }

    // 외부 초기화
    public void Init()
    {
        InitializedSlot();

        if (_button != null)
            _button.onClick.RemoveAllListeners();
    }



    #region 내부 동작 메서드

    // 스프라이트 출력
    private void ShowSprite(int id, out ItemDataBase item)
    {
        item = ItemDB.GetData(id);
        if(item != null)
            _image.sprite = item.SpriteImg;
    }

    // 텍스트 출력(string)
    private void ShowText(string value = null)
    {
        if (_amountText == null)
            return;

        if (value != null)
            _amountText.text = value.ToString();
        else
            _amountText.text = string.Empty;
    }

    // 텍스트 출력(int)
    private void ShowText(int value)
    {
        string text = value.ToString();
        ShowText(text);
    }

    // 슬롯의 스프라이트, 텍스트 초기화
    private void InitializedSlot()
    {
        _image.sprite = _baseSprite;

        if (_amountText != null)
            _amountText.text = string.Empty;
    }

    #endregion
}