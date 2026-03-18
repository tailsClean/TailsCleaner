using UnityEngine;

/// <summary>
/// 추가 수정 필요
/// </summary>
public class Currency : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeGold;
    [SerializeField] private IntEventChannelSO _onSellingItem;

    [SerializeField] private int _goldAmount = 1000;

    public int GoldAmount
    {
        get
        {
            if (_goldAmount < 0)
                Debug.LogError("현재 Money가 음수입니다.");
            return Mathf.Max(_goldAmount, 0);
        }
    }

    private void Awake()
    {
        _onSellingItem.AddListener(GainGold);
    }

    private void OnDestroy()
    {
        _onSellingItem.RemoveListener(GainGold);
    }


    public ItemInstance GetGold()
    {
        var gold = new ItemInstance(ItemID.Gold);
        gold.SetAmount(GoldAmount);
        return gold;
    }

    public void GainGold(int amount)
    {
        _goldAmount += amount;
        _onChangeGold.OnStartEvent();
    }

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
        _onChangeGold.OnStartEvent();
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
}