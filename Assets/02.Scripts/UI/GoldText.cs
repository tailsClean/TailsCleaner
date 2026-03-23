using UnityEngine;
using TMPro;
public class GoldText: MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _onChangeGold;
    [SerializeField] private ItemCurrency _currency;
    [SerializeField] private TextMeshProUGUI _goldTxt;

    private void OnEnable()
    {
        _currency = FindAnyObjectByType<ItemCurrency>();
        RefreshTxt();
        _onChangeGold.AddListener(RefreshTxt);
    }
    private void OnDisable()
    {
        _onChangeGold.RemoveListener(RefreshTxt);
    }

    private void RefreshTxt()
    {
        _goldTxt.text = $"{_currency.GoldAmount}";
    } 


}
