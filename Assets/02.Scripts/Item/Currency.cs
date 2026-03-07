using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Currency : MonoBehaviour
{
    [SerializeField] private int _goldAmount = 1000;
    public int MoneyID { get; private set; } = 15;

    public int GoldAmount
    {
        get
        {
            if (_goldAmount < 0)
                Debug.LogError("현재 Money가 음수입니다.");
            return Mathf.Max(_goldAmount, 0);
        }

    }


    public void GainGold(int amount) => _goldAmount += amount;
    public void UseGold(int amount)
    {
        if (amount < 0)
            return;

        if(_goldAmount < amount)
        {
            Debug.LogWarning("사용금액이 현재 금액을 초과합니다.");
            return;
        }

        _goldAmount -= amount;
    }
    public bool TryUseGold(int amount)
    {
        if (amount < 0)
            return false;

        if(_goldAmount < amount)
        {
            Debug.LogWarning("사용금액이 현재 금액을 초과합니다.");
            return false;
        }

        return true;
    }




    #region Money UI
    //
    public Image MoneyImage;
    private void Update()
    {
        SetMoney();
    }
    public TextMeshProUGUI MoneyText;

    public void SetMoney()
    {
        MoneyImage.sprite = ItemDB.GetItem<StackableItem>(MoneyID).GetSprite();
        MoneyText.text = GoldAmount.ToString();
    }

    //
    #endregion
}