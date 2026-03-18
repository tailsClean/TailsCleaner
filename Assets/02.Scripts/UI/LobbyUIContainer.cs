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
    [SerializeField] private Button _stageButton;
    [SerializeField] private Button _settingButton;
    
     
    
    void Start()
    {
        _dungeonButton.onClick.AddListener(UIManager.Instance.ChangeStateDungeonSelect);
        _stageButton.onClick.AddListener(UIManager.Instance.GoToStage);
        _settingButton.onClick.AddListener(() => {
        UIManager.Instance.ChangeStateSettingPanel();
    });
    }

}
