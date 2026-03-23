using System;
using UnityEngine;

public class EnhanceSystem : MonoBehaviour
{
    [SerializeField] private ItemCurrency _currency;                // 필요 금화를 읽어올 재화 가방
    [SerializeField] private ItemInventory _inventory;


    private PlayerLoadout _playerLoadout;                       // 장착 장비 확인을 위함
    private IEnhancement _settingItem;
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

        //OnEnhance -= _settingItem != null ? _settingItem.OnEnhance : null;
        _settingItem = equipment;

        _enhanceInfo = new EnhancingInfo(equipment.Data.Equipmnet.id, equipment.EnhanceLevel);

        _bluePrintID = equipment.EnhanceData.blueprint_id;
        _bluePrintCost = equipment.EnhanceData.cost_blueprint;

        //OnEnhance += _settingItem.OnEnhance;
        OnSetEquipment?.Invoke(_enhanceInfo);
    }

    // 강화할 유물 세팅
    public void SetRelic(int id, int enhanceLevel)
    {
        ItemInstance item = _inventory.GetRelic(id, enhanceLevel);
        _enhanceInfo = new EnhancingInfo(id, enhanceLevel, item);
        //OnEnhance -= _settingItem != null ? _settingItem.OnEnhance : null;
        

        _bluePrintID = _enhanceInfo.EnhanceData.BluePrintID;
        _bluePrintCost = _enhanceInfo.EnhanceData.CostBluePrint;
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
            _currency.UseGold(_enhanceInfo.EnhanceData.CostGold);
            _inventory.UseStackItem(_bluePrintID, _bluePrintCost);

            _enhanceInfo.EnhanceLevel = _enhanceInfo.EnhanceLevel + 1;
            switch(_enhanceInfo.ItemType)
            {
                case ITEM_TYPE.Equipment:
                    if (ItemDB.TryGetData<DefaultEquipData>(_enhanceInfo.ItemID, out var equipData))
                        _playerLoadout.MyEquipments[equipData.Equipmnet.part].OnEnhance(_enhanceInfo);
                    break;

                case ITEM_TYPE.Relic:
                    var itemInstance = _enhanceInfo.InventoryKey;
                    _inventory.RemoveRelic(itemInstance.ID, itemInstance.EnhanceLevel);
                    _inventory.GainRelic(itemInstance.ID, itemInstance.EnhanceLevel + 1);
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

        _isEnhancable = !_enhanceInfo.EnhanceData.IsMaxLevel;

        if (!_isEnhancable)
            Debug.Log("강화수치가 최대입니다.");
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if (!_isEnhancable)
            return;

        int cost = _enhanceInfo.EnhanceData.CostGold;

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

        var item = _inventory.GetStackItem(_bluePrintID);
        _isEnhancable = item.Amount >= _bluePrintCost;

        if (!_isEnhancable)
            Debug.Log("강화 재료가 부족합니다.");
    }
}

public class EnhancingInfo
{
    public ItemInstance InventoryKey;
    public int ItemID;
    public int EnhanceLevel;
    public ITEM_TYPE ItemType;
    public ItemEnhanceData EnhanceData
    {
        get
        {
            switch(ItemType)
            {
                case ITEM_TYPE.Equipment:
                    var data1 = ItemDB.GetData(ItemID);
                    if (data1.TryGetData<DefaultEquipData>(out var equipData))
                        return new ItemEnhanceData(equipData, EnhanceLevel + 1);

                    return default;

                case ITEM_TYPE.Relic:
                    var data2 = ItemDB.GetData(ItemID);
                    if (data2.TryGetData<RelicData>(out var relicData))
                        return new ItemEnhanceData(relicData, EnhanceLevel + 1);

                    return default;
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
        ItemID = itemID;
        EnhanceLevel = enhanceLevel;
        ItemType = ItemDB.GetData(itemID).Type;
    }

    /// <summary>
    /// 착용 장비에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="enhanceLevel"></param>
    public EnhancingInfo(int itemID, int enhanceLevel)
    {
        ItemID = itemID;
        EnhanceLevel = enhanceLevel;
        ItemType = ItemDB.GetData(itemID).Type;
    }

    public bool IsEquals(ItemInstance item)
    {
        return InventoryKey.ID == item.ID &&
       InventoryKey.EnhanceLevel == item.EnhanceLevel &&
       InventoryKey.Grade == item.Grade;
    }
}