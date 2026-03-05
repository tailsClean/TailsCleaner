using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Text text;
    public Image image;
    public Sprite baseSprite;

    private void Awake()
    {
        image = GetComponent<Image>();
        baseSprite = image.sprite;
    }

    public void SetIcon(int id, int amount)
    {
        //if(amount == 0)
        //{
        //    ItemID = 0;
        //    Amount = 0;
        //    image.sprite = baseSprite;
        //    text.text = string.Empty;
        //    return;
        //}

        var a = ItemDB.GetItem<ItemBase>(id);
        if(a != null)
        {
            image.sprite = a.GetSprite();
            text.text = amount.ToString();
        }
        else
        {
            Debug.Log("값을 찾을 수 없다.");
            image.sprite = baseSprite;
            text.text = string.Empty;
        }

    }

    public void Init()
    {
        image.sprite = baseSprite;
        text.text = string.Empty;
    }
}