using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountText;

    private Image image;
    private Sprite baseSprite;

    private void Awake()
    {
        image = GetComponent<Image>();
        baseSprite = image.sprite;
    }

    // 슬롯에 아이템(ID)과 갯수를 표시
    public void SetSlot(int id, int amount)
    {
        var a = ItemDB.GetItem<ItemBase>(id);
        if(a != null)
        {
            image.sprite = a.GetSprite();
            _amountText.text = amount.ToString();
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            image.sprite = baseSprite;
            _amountText.text = string.Empty;
        }

    }

    public void Init()
    {
        image.sprite = baseSprite;
        _amountText.text = "갯수";
    }
}