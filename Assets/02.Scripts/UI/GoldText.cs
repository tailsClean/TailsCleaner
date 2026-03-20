using UnityEngine;
using TMPro;
public class GoldText: MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO _onChangeGold;
    [SerializeField] private Currency _currency;
    [SerializeField] private TextMeshProUGUI _goldTxt;

    private void OnEnable()
    {
        _currency = FindAnyObjectByType<Currency>();
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
