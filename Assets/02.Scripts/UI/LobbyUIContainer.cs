using UnityEngine;
using UnityEngine.UI;

public class LobbyUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button _dungeonButton;
    [SerializeField] private EnergyPanel _energyPanel;
    public EnergyPanel EnergyPanel => _energyPanel;
    [SerializeField] private GameObject _dungeonSelect;
    public GameObject DungeonSelect => _dungeonSelect;
    [SerializeField] private GameObject _stageSelect;
    public GameObject StageSelect => _stageSelect;
    [SerializeField] private Button _settingButton;
    [SerializeField] private Button _relicButton;
    [SerializeField] private Button _equipmentButton;
    [SerializeField] private Button _inventoryButton;
     
     
    
    void Start()
    {
        _dungeonButton.onClick.AddListener(UIManager.Instance.ChangeStateDungeonSelect);
        _settingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        _relicButton.onClick.AddListener(UIManager.Instance.ChangeStateRelic);
        _equipmentButton.onClick.AddListener(UIManager.Instance.ChangeStateEquipUI);
        _inventoryButton.onClick.AddListener(UIManager.Instance.ChangeStateInventory);
    }

}
