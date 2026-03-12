using System;
using UnityEngine;
using static EquipmentSO;


public partial class CraftingSystem : MonoBehaviour
{
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Currency _currency;        // 필요 금화를 읽어올 재화 가방


    private PlayerLoadout _loadout;                     // 장착 장비 확인을 위함
    private CraftingInfo _mainCraftSlot;
    private CraftingInfo[] _resourceCraftSlots;



    public PlayerLoadout Loadout => _loadout;
    public Inventory UsingInventory => _inventory;
    public Currency UsingCurrency => _currency;
    public CraftingInfo MainEquip => _mainCraftSlot;
    public CraftingInfo[] ResourceEquips => _resourceCraftSlots;

    public event Action OnSetEquipment;
    public event Action<CraftingInfo> OnCrafting;


    private void Start()
    {
        _loadout = ItemManager.Instance.Loadout;
    }



    public void SetCraftSlot(CraftingInfo equipment)
    {
        if(_mainCraftSlot == null)
        {
            SetMainCraftSlot(equipment);
            return;
        }

        SetResourceCraftSlots(equipment);
    }

    // 강화 장비칸에 장비 세팅
    private void SetMainCraftSlot(CraftingInfo mainEquip)
    {
        if(mainEquip == null)
        { Debug.LogError("설정하려는 아이템이 없습니다."); return; }

        _mainCraftSlot = mainEquip;
        _resourceCraftSlots = new CraftingInfo[mainEquip.GradeData.CostCount];

        OnSetEquipment?.Invoke();
    }

    // 재료 장비칸에 장비 세팅
    private void SetResourceCraftSlots(CraftingInfo resourceEquip)
    {
        if (_mainCraftSlot == null || _resourceCraftSlots == null)
        { Debug.LogWarning("강화 장비부터 세팅하세요."); return; }

        if(!CheckGrade(resourceEquip))
        { Debug.Log("<color=yellow>재료 장비의 등급이 적합하지 않습니다.</color>");  return; }

        if(!CheckParts(resourceEquip))
        { Debug.Log("<color=yellow>재료 장비의 부위가 다릅니다..</color>");  return; }

        for(int i = 0; i < _resourceCraftSlots.Length; i++)
        {
            CraftingInfo slot = _resourceCraftSlots[i];
            if(slot == null)
            {
                _resourceCraftSlots[i] = resourceEquip;
                OnSetEquipment?.Invoke();
                return;
            }
        }
        Debug.Log("<color=yellow>재료 장비칸이 가득 찼습니다.</color>");
    }
    // 재료의 등급 확인
    private bool CheckGrade(CraftingInfo resourceEquip) => _mainCraftSlot.Grad == resourceEquip.Grad;
    // 재료의 부위 확인
    private bool CheckParts(CraftingInfo resourceEquip) => _mainCraftSlot.Parts == resourceEquip.Parts;


    // 합성 시작
    public void OnStartCrafting()
    {
        // 최대 등급 확인
        if (_mainCraftSlot.GradeData.IsMaxGrade)
        { Debug.Log("최대 등급의 장비입니다."); return; }

        // 필요 재료 갯수 확인
        if (Array.Exists(_resourceCraftSlots, x => x == null))
        { Debug.Log("재료장비의 개수가 부족합니다."); return;}


        _mainCraftSlot.Grad++;
        if(_mainCraftSlot.IsLoadout)
        {
            var parts = _mainCraftSlot.Parts;
            _loadout.MyEquipments[parts].OnUpgrade();
        }
        else
        {
            var equipStatus = new EquipStatus(_mainCraftSlot.InstanceID, _mainCraftSlot.ItemID, _mainCraftSlot.Grad);
            _inventory.SetEquipment(equipStatus);
        }


        ReleaseResource();
        OnCrafting?.Invoke(_mainCraftSlot);
        Debug.Log("등급 업그레이드 성공!");
    }

    
    // 합성 리스트에서 특정 장비 제거
    public void RemoveMainEquip(CraftingInfo equip)
    {
        if(_mainCraftSlot != null && _mainCraftSlot.InstanceID == equip.InstanceID)
        {
            _mainCraftSlot = null;
            _resourceCraftSlots = null;
        }
    }

    public void RemoveResourceEquip(CraftingInfo equip)
    {
        if (_resourceCraftSlots == null)
            return;

        for (int i = _resourceCraftSlots.Length; i > 0; i--)
        {
            var slot = _resourceCraftSlots[i - 1];
            if (slot != null && slot.InstanceID == equip.InstanceID)
            {
                _resourceCraftSlots[i - 1] = null;
                return;
            }
        }

        Debug.Log("지우려는 아이템이 합성 리스트에 없습니다.");
    }



    private void ReleaseResource()
    {
        foreach(var slot in _resourceCraftSlots)
        {
            var equipStatus = new EquipStatus(slot.InstanceID, slot.ItemID, slot.Grad);
            _inventory.RemoveEquipment(equipStatus);
        }

        _resourceCraftSlots = null;
    }


    
}

public class CraftingInfo
{
    public EQUIP_GRADE Grad;
    public readonly int InstanceID;
    public readonly int ItemID;
    public readonly EQUIP_PARTS Parts;
    public readonly bool IsLoadout;
    public int CostID => GradeData.CostID;
    public EquipGradeData GradeData => ItemDB.GetItemData<EquipmentSO>(ItemID).GetGradeData(Grad);

    public CraftingInfo(int itemID, EQUIP_GRADE grad, int instanceID = 0)
    {
        InstanceID = instanceID;
        ItemID = itemID;
        Grad = grad;
    }

    public CraftingInfo(EquipStatus inventoryEuipment)
    {
        InstanceID = inventoryEuipment.InstanceID;
        ItemID = inventoryEuipment.UniqueID;
        Grad = inventoryEuipment.Grade;
        Parts = inventoryEuipment.Parts;
        IsLoadout = false;
    }

    public CraftingInfo(EquipmentBase equipment )
    {
        ItemID = equipment.Data.UniqueID;
        Grad = equipment.Grade;
        Parts = equipment.EquipmentPart;
        IsLoadout = true;
    }
}