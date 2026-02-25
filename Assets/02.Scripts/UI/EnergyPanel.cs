using UnityEngine;
using TMPro;

public class EnergyPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _energyText;
    [SerializeField] private VoidEventChannelSO _OnStartInGame;
    private void Start()
    {
        UpdateEnergyText();
        _OnStartInGame.AddListener(UpdateEnergyText);
    }
    private void OnDestroy()
    {
        _OnStartInGame.RemoveListener(UpdateEnergyText);
    }
   
    public void UpdateEnergyText()
    {
        _energyText.text = $"{GameManager.EnergyCount}/{GameManager.Instance._energySystem.MaxEnergy}";
    } 







}
