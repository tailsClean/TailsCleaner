using System;
using UnityEngine;

public class EnhanceSystem : MonoBehaviour
{
    [SerializeField] private Currency _currency;                // 필요 금화를 읽어올 재화 가방
    [SerializeField] private Inventory _inventory;


    private PlayerLoadout _playerLoadout;                       // 장착 장비 확인을 위함
    private EquipmentBase _enhanceItem;
    private int _bluePrintID;
    private int _bluePrintCost;
    private bool _isEnhancable;                                 // 강화 가능 여부 판단

    public Inventory UsingInventory => _inventory;
    public Currency UsingCurrency => _currency;

    public event Action<EquipmentBase> OnSetEquipment;
    public event Action<EquipmentBase> OnEnhace;

    private void Start()
    {
        _playerLoadout = ItemManager.Instance.Loadout;
    }


    // 강화할 아이템 세팅
    public void SetEquipment(EQUIP_PARTS part)
    {
        EquipmentBase equipment = _playerLoadout.MyEquipments[part];
        _enhanceItem = equipment;
        _bluePrintID = equipment.EnhanceData.BluePrintID;
        _bluePrintCost = equipment.EnhanceData.CostBluePrint;
        OnSetEquipment?.Invoke(equipment);
    }


    public void OnEnhance()
    {
        _isEnhancable = true;
        CheckMaxLevel();
        CheckGoldCost();
        CheckResourceCost();

        if (_isEnhancable)
        {
            _enhanceItem.OnEnhance();

            _currency.UseGold(_enhanceItem.EnhanceData.CostGold);
            _inventory.UseItem(ITEM_TYPE.Reinforcement, _bluePrintID, _bluePrintCost);
            OnEnhace?.Invoke(_enhanceItem);
            Debug.Log("강화성공!");
        }
    }


    // 최대 강화수치인지 확인
    private void CheckMaxLevel()
    {
        if (!_isEnhancable)
            return;

        _isEnhancable = !_enhanceItem.EnhanceData.IsMaxLevel;

        if (!_isEnhancable)
            Debug.Log("강화수치가 최대입니다.");
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if (!_isEnhancable)
            return;

        int cost = _enhanceItem.EnhanceData.CostGold;

        if (cost < 0)
        { Debug.LogError("골드 코스트가 음수입니다!"); return; }

        _isEnhancable = _currency.TryUseGold(cost);

        if (!_isEnhancable)
            Debug.Log("골드가 부족합니다.");
    }


    // 재료 재화와 코스트 비교
    private void CheckResourceCost()
    {
        if (!_isEnhancable)
            return;

        if (_bluePrintCost < 0)
        { Debug.LogError("재료 코스트가 음수입니다!"); return; }

        _isEnhancable = _inventory.TryUseItem(ITEM_TYPE.Reinforcement, _bluePrintID, _bluePrintCost);

        if (!_isEnhancable)
            Debug.Log("강화 재료가 부족합니다.");
    }
}
