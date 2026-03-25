
using UnityEngine;
using UnityEngine.UI;


public class EquipedPopup : ItemPopup
{
    [Header("장착 버튼")]
    [SerializeField] private Button _equipedButton;

    private PlayerLoadout _playerLoadout;


    protected override void Start()
    {
        base.Start();
        _playerLoadout = ItemManager.Instance.Loadout;
    }

    public override void SetSlot(ItemInstance item)
    {
        base.SetSlot(item);
        SetEquipedButton();
    }

    private void SetEquipedButton()
    {
        _equipedButton.onClick.RemoveAllListeners();
        _equipedButton.onClick.AddListener(SetRelic);
        _equipedButton.onClick.AddListener(EquipedNext);
    }

    private void SetRelic()
    {
        _playerLoadout.SetRelic(_currentItem);
    }

    private void EquipedNext() => transform.gameObject.SetActive(false);
}