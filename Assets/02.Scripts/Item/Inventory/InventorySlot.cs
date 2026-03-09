using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 수정 필요
/// </summary>
[RequireComponent(typeof(Button))]
public class InventorySlot : MonoBehaviour
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

    // 슬롯에 아이템(ID)과 갯수를 표시
    public void SetSlot(int id, int amount)
    {
        var a = ItemDB.GetItemSO<ItemBaseSO>(id);
        if (a != null)
        {
            _image.sprite = a.ImageSprite;
            _amountText.text = amount.ToString();
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            _image.sprite = _baseSprite;
            _amountText.text = string.Empty;
        }
    }

    public void SetSlot(int id, int amount, Action action)
    {
        SetSlot(id, amount);
        AddListner(action);
    }

    private void AddListner(Action action) => _button.onClick.AddListener(action.Invoke);

    public void Init()
    {
        _image.sprite = _baseSprite;
        _amountText.text = "갯수";
        _button.onClick = null;
    }
}