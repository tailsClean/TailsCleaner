using System;
using UnityEngine;

public class EnhanceSystem : MonoBehaviour
{
    [SerializeField] private ItemCurrency _currency;            // 필요 금화를 읽어올 재화 가방
    [SerializeField] private ItemInventory _inventory;


    private PlayerLoadout _playerLoadout;                       // 장착 장비 확인을 위함
    private EnhancingInfo _enhanceInfo;
    private int _bluePrintID;
    private int _bluePrintCost;
    private bool _isEnhancable;                                 // 강화 가능 여부 판단

    public ItemInventory UsingInventory => _inventory;
    public ItemCurrency UsingCurrency => _currency;

    public event Action<EnhancingInfo> OnSetEquipment;
    public event Action<EnhancingInfo> OnEnhance;

    private void Start()
    {
        _playerLoadout = ItemManager.Instance.Loadout;
    }


    // 강화할 장비 세팅
    public void SetEquipment(PART part)
    {
        EquipmentBase equipment = _playerLoadout.MyEquipments[part];

        _enhanceInfo = new EnhancingInfo(equipment.Data.Equipmnet.id, equipment.CurrentEnhanceLevel);

        _bluePrintID = equipment.CurrentEnhanceData.blueprint_id;
        _bluePrintCost = equipment.CurrentEnhanceData.cost_blueprint;

        OnSetEquipment?.Invoke(_enhanceInfo);
    }

    // 강화할 유물 세팅
    public void SetRelic(int id, int enhanceLevel)
    {
        ItemInstance item = _inventory.GetRelic(id, enhanceLevel);
        _enhanceInfo = new EnhancingInfo(id, enhanceLevel, item);

        _bluePrintID = _enhanceInfo.NextEnhanceData.BluePrintID;
        _bluePrintCost = _enhanceInfo.NextEnhanceData.CostBluePrint;
        OnSetEquipment?.Invoke(_enhanceInfo);
    }

    public void OnStartEnhance()
    {
        _isEnhancable = true;
        CheckMaxLevel();
        CheckGoldCost();
        CheckResourceCost();

        if (_isEnhancable)
        {
            _currency.UseGold(_enhanceInfo.NextEnhanceData.CostGold);
            _inventory.UseStackItem(_bluePrintID, _bluePrintCost);

            _enhanceInfo.CurrentEnhanceLevel++;
            switch (_enhanceInfo.ItemType)
            {
                case ITEM_TYPE.Equipment:
                    if (ItemDB.TryGetData<DefaultEquipData>(_enhanceInfo.ItemID, out var equipData))
                        _playerLoadout.MyEquipments[equipData.Equipmnet.part].OnEnhance(_enhanceInfo);
                    break;

                case ITEM_TYPE.Relic:
                    var itemInstance = _enhanceInfo.InventoryKey;
                    _inventory.RemoveRelic(itemInstance.ID, itemInstance.EnhanceLevel);
                    _inventory.GainRelic(itemInstance.ID, itemInstance.EnhanceLevel + 1);
                    SetRelic(itemInstance.ID, itemInstance.EnhanceLevel + 1);
                    break;
            }


            OnEnhance?.Invoke(_enhanceInfo);
            Debug.Log("강화성공!");
        }
    }


    // 최대 강화수치인지 확인
    private void CheckMaxLevel()
    {
        if (!_isEnhancable)
            return;

        _isEnhancable = !_enhanceInfo.IsCurrentMaxLevel;

        if (!_isEnhancable)
        {
            WarningText.ShowText("강화수치가 최대입니다.");
            Debug.Log("강화수치가 최대입니다.");
        }
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if (!_isEnhancable)
            return;

        int cost = _enhanceInfo.NextEnhanceData.CostGold;

        if (cost < 0)
        {
            WarningText.ShowText("골드 코스트가 음수입니다!");
            Debug.LogError("골드 코스트가 음수입니다!"); return; 
        }

        _isEnhancable = _currency.TryUseGold(cost);

        if (!_isEnhancable)
        {
            WarningText.ShowText("골드가 부족합니다.");
            Debug.Log("골드가 부족합니다.");
        }
    }


    // 재료 재화와 코스트 비교
    private void CheckResourceCost()
    {
        if (!_isEnhancable)
            return;

        if (_bluePrintCost < 0)
        { Debug.LogError("재료 코스트가 음수입니다!"); return; }

        var item = _inventory.GetStackItem(_bluePrintID);
        _isEnhancable = item.Amount >= _bluePrintCost;

        if (!_isEnhancable)
        {
            WarningText.ShowText("강화 재료가 부족합니다.");
            Debug.Log("강화 재료가 부족합니다.");
        }
    }
}

public class EnhancingInfo
{
    private DefaultEquipData _equipData;
    private RelicData _relicData;

    public readonly ItemInstance InventoryKey;
    public readonly int ItemID;
    public int CurrentEnhanceLevel;
    public ITEM_TYPE ItemType;

    public bool IsCurrentMaxLevel
    {
        get
        {
            switch (ItemType)
            {
                case ITEM_TYPE.Equipment:
                    return _equipData.GetEnhance(CurrentEnhanceLevel).is_max_level;

                case ITEM_TYPE.Relic:
                    return _relicData.GetEnhance(CurrentEnhanceLevel).is_max_level;
            }
            return default;
        }
    }

    public ItemEnhanceData NextEnhanceData
    {
        get
        {
            switch (ItemType)
            {
                case ITEM_TYPE.Equipment:
                    if (!IsCurrentMaxLevel)
                        return new ItemEnhanceData(_equipData, CurrentEnhanceLevel + 1);
                    break;

                case ITEM_TYPE.Relic:
                    if (!IsCurrentMaxLevel)
                        return new ItemEnhanceData(_relicData, CurrentEnhanceLevel + 1);

                    break;
            }
            return default;
        }
    }

    /// <summary>
    /// 인벤토리에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="enhanceLevel"></param>
    /// <param name="inventoryKey"></param>
    public EnhancingInfo(int itemID, int enhanceLevel, ItemInstance inventoryKey)
    {
        InventoryKey = inventoryKey;

        var itemData = ItemDB.GetData(itemID);
        ItemDB.TryGetData(itemData, out _equipData);
        ItemDB.TryGetData(itemData, out _relicData);

        ItemID = itemID;
        CurrentEnhanceLevel = enhanceLevel;
        ItemType = itemData.Type;
    }

    /// <summary>
    /// 착용 장비에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="enhanceLevel"></param>
    public EnhancingInfo(int itemID, int enhanceLevel)
    {
        var itemData = ItemDB.GetData(itemID);
        ItemDB.TryGetData(itemData, out _equipData);
        ItemDB.TryGetData(itemData, out _relicData);

        ItemID = itemID;
        CurrentEnhanceLevel = enhanceLevel;
        ItemType = itemData.Type;
    }

    public bool IsEquals(ItemInstance item)
    {
        return InventoryKey.ID == item.ID &&
       InventoryKey.EnhanceLevel == item.EnhanceLevel &&
       InventoryKey.Grade == item.Grade;
    }
}