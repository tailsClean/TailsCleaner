using UnityEngine;
using TMPro;

public class EnergyPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _energyText;
    [SerializeField] private VoidEventChannelSO _OnEnergyChange;
    private void Start()
    {
        _OnEnergyChange.AddListener(UpdateEnergyText);
        UpdateEnergyText();
    }
    private void OnDestroy()
    {
        _OnEnergyChange.RemoveListener(UpdateEnergyText);
    }
   
    public void UpdateEnergyText()
    {
        _energyText.text = $"{GameManager.Instance.EnergyCount}/{GameManager.Instance._energySystem.MaxEnergy}";
    } 
}
