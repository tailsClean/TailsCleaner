using System;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUIContainer : MonoBehaviour, IUIContainer
{
    // [SerializeField] private GameObject _dungeonSelect;
    // public GameObject DungeonSelect => Current.DungeonSelect;
    // [SerializeField] private GameObject _stageSelect;
    // public GameObject StageSelect => _stageSelect;

    [Header("가로 UI")]
    [SerializeField] private LobbyUIReference _horizontal;

    [Header("세로 UI")]
    [SerializeField] private LobbyUIReference _vertical;

    public LobbyUIReference Current => UIManager.Instance.IsVertical ? _vertical : _horizontal;
    public GameObject DungeonSelect => Current.DungeonSelect;
    public GameObject StageSelect => Current.StageSelect;
    public Button DungeonButton => Current.DungeonButton;
    public Button SettingButton => Current.SettingButton;
    public Button RelicButton => Current.RelicButton;
    public Button EquipmentButton => Current.EquipmentButton;
    public Button InventoryButton => Current.InventoryButton;
    public Button MyStatsButton => Current.MyStatsButton;

    void Start()
    {
        BindButtons(Current);

        OnOrientationChanged(UIManager.Instance.IsVertical);
        UIManager.Instance.OnOrientationChanged += OnOrientationChanged;
    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnOrientationChanged -= OnOrientationChanged;
    }

    private void OnOrientationChanged(bool isVertical)
    {
        UIManager.Instance.UpdateLobbyUIReference(this);
    }

    private void BindButtons(LobbyUIReference reference)
    {
        reference.DungeonButton.onClick.AddListener(UIManager.Instance.ChangeStateDungeonSelect);
        reference.SettingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        reference.RelicButton.onClick.AddListener(UIManager.Instance.ChangeStateRelic);
        reference.EquipmentButton.onClick.AddListener(UIManager.Instance.ChangeStateEquipUI);
        reference.InventoryButton.onClick.AddListener(UIManager.Instance.ChangeStateInventory);
        reference.MyStatsButton.onClick.AddListener(UIManager.Instance.ChangeStateMyStatsUI);
    }
}

[Serializable]
public class LobbyUIReference
{
    public GameObject DungeonSelect;
    public GameObject StageSelect;
    public Button DungeonButton;
    public Button SettingButton;
    public Button RelicButton;
    public Button EquipmentButton;
    public Button InventoryButton;
    public Button MyStatsButton;
}
