using UnityEngine;
using UnityEngine.UI;

public class TowerUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button stageButton;
    [SerializeField] private GameObject _energyPanel;
    public GameObject EnergyPanel => _energyPanel;

    void Start()
    {
        stageButton.onClick.AddListener(() => GameManager.Instance.EnterStage());
    }

}
