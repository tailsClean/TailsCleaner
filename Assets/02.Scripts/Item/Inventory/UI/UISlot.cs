using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Button;

/// <summary>
/// 수정 필요
/// </summary>
[RequireComponent(typeof(Button))]
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
        var a = ItemDB.GetItemData<ItemBaseSO>(id);
        if (a != null)
        {
            _image.sprite = a.ImageSprite;
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

    public void AddListener(Action action) =>_button.onClick.AddListener(() => action());


    public void Init()
    {
        _image.sprite = _baseSprite;
        _amountText.text = string.Empty;
        _button.onClick.RemoveAllListeners();
    }
}