using System;
using UnityEngine;

public class EnhanceSystem : MonoBehaviour
{
    private ItemCurrency _currency;                             // 필요 금화를 읽어올 재화 가방
    private ItemInventory _inventory;
    private PlayerLoadout _playerLoadout;                       // 장착 장비 확인을 위함
    private EnhancingInfo _enhanceInfo;
    private int _bluePrintID;
    private int _bluePrintCost;
    private bool _isEnhancable;                                 // 강화 가능 여부 판단

    public event Action<EnhancingInfo> OnSetEquipment;
    public event Action<EnhancingInfo> OnEnhance;

    private void Start()
    {
        _currency = ItemManager.Instance.Currency;
        _inventory = ItemManager.Instance.Inventory;
        _playerLoadout = PlayerStatManager.Instance.Loadout;
    }


    // 강화할 장비 세팅
    public void SetEquipment(PART part)
    {
        EquipmentBase equipment = _playerLoadout.MyEquipments[part];

        _enhanceInfo = new EnhancingInfo(equipment);

        _bluePrintID = _enhanceInfo.NextEnhanceData.BluePrintID;
        _bluePrintCost = _enhanceInfo.NextEnhanceData.CostBluePrint;

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

            _enhanceInfo.EnhanceLevel++;
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
            EnhanceSuccessSound();
            Debug.Log("강화성공!");
        }
        else
        {
            EnhanceFailSound();
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
            OnImpossiblePanel("더 이상 강화할 수 없는 장비에요!");
        }
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if (!_isEnhancable)
            return;

        int cost = _enhanceInfo.NextEnhanceData.CostGold;

        if (cost < 0)
        { Debug.LogError("골드 코스트가 음수입니다!"); return; }

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
            OnImpossiblePanel("강화에 사용할 아이템이 부족해요!");
        }
    }
    
    // 강화 성공 사운드
    private void EnhanceSuccessSound()
    {
        if (SoundManager.Instance) SoundManager.Instance.PlayUISFX(UISFXName.EnhanceSuccess);
    }

    // 강화 성공 사운드
    private void EnhanceFailSound()
    {
        if (SoundManager.Instance) SoundManager.Instance.PlayUISFX(UISFXName.EnhanceFail);
    }

    // 강화 불가능 패널 띄우기
    private void OnImpossiblePanel(string text)
    {
        UIManager.Instance.ChangeStateImpossiblePanel();
        var impossiblePanel = UIManager.Instance.ImpossiblePanel;

        impossiblePanel.SetText(text);
        impossiblePanel.SetListeners(() => impossiblePanel.gameObject.SetActive(false));

    }
}

public class EnhancingInfo
{
    private DefaultEquipData _equipData;
    private RelicData _relicData;

    public readonly ItemInstance InventoryKey;
    public readonly int ItemID;
    public int EnhanceLevel;
    public ITEM_TYPE ItemType;

    public bool IsCurrentMaxLevel
    {
        get
        {
            switch (ItemType)
            {
                case ITEM_TYPE.Equipment:
                    var equipEnhance = _equipData.GetEnhance(EnhanceLevel);
                    return equipEnhance != null ? equipEnhance.is_max_level : false;

                case ITEM_TYPE.Relic:
                    var relicEnhance = _relicData.GetEnhance(EnhanceLevel);
                    return relicEnhance != null ? relicEnhance.is_max_level : false;
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
                        return new ItemEnhanceData(_equipData, EnhanceLevel + 1);
                    break;

                case ITEM_TYPE.Relic:
                    if (!IsCurrentMaxLevel)
                        return new ItemEnhanceData(_relicData, EnhanceLevel + 1);

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
        EnhanceLevel = enhanceLevel;
        ItemType = itemData.Type;
    }

    /// <summary>
    /// 착용 장비에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="enhanceLevel"></param>
    public EnhancingInfo(EquipmentBase equipment)
    {
        InventoryKey = new ItemInstance(equipment.Data.UniqueID, equipment.EnhanceLevel, equipment.CurrentGrade);

        var itemData = ItemDB.GetData(equipment.Data.UniqueID);
        ItemDB.TryGetData(itemData, out _equipData);
        ItemDB.TryGetData(itemData, out _relicData);

        ItemID = equipment.Data.UniqueID;
        EnhanceLevel = equipment.EnhanceLevel;
        ItemType = itemData.Type;
    }

    public bool IsEquals(ItemInstance item)
    {
        return InventoryKey.ID == item.ID &&
       InventoryKey.EnhanceLevel == item.EnhanceLevel &&
       InventoryKey.Grade == item.Grade;
    }
}