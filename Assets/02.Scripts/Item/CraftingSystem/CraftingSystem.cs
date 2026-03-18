using System;
using UnityEngine;


public partial class CraftingSystem : MonoBehaviour
{
    [SerializeField] private ItemInventory _inventory;


    private PlayerLoadout _loadout;                     // 장착 장비 확인을 위함
    private CraftingInfo _mainCraftSlot;
    private CraftingInfo[] _resourceCraftSlots;


    public ItemInventory UsingInventory => _inventory;
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

        if(equipment.IsLoadout)
        { Debug.Log("<color=yellow>착용장비는 재료로 사용할 수 없습니다.</color>"); return; }

        SetResourceCraftSlots(equipment);
    }

    // 업그레이드 장비칸에 장비 세팅
    private void SetMainCraftSlot(CraftingInfo mainEquip)
    {
        if(mainEquip == null)
        { Debug.LogError("설정하려는 아이템이 없습니다."); return; }

        _mainCraftSlot = mainEquip;
        _resourceCraftSlots = new CraftingInfo[mainEquip.GradeData.cost_count];

        OnSetEquipment?.Invoke();
    }

    // 재료 장비칸에 장비 세팅
    private void SetResourceCraftSlots(CraftingInfo resourceEquip)
    {
        if (_mainCraftSlot == null || _resourceCraftSlots == null)
        { Debug.LogWarning("합성 장비부터 세팅하세요."); return; }

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
        if (_mainCraftSlot.GradeData.is_max_grade)
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
            EquipRemoveAndCreate();
        }

        ReleaseResource();
        OnCrafting?.Invoke(_mainCraftSlot);
        Debug.Log("등급 업그레이드 성공!");
    }
    // 합성된 장비 삭제 및 업그레이드 장비 추가
    private void EquipRemoveAndCreate()
    {
        var originalEquip = _mainCraftSlot.InventoryKey;
        _inventory.RemoveEquipment(originalEquip.ID, originalEquip.Grade);
        _inventory.GainEquipment(originalEquip.ID, originalEquip.Grade + 1);

        foreach(var slot in _resourceCraftSlots)
        {
            ItemInstance resource = slot.InventoryKey;
            _inventory.RemoveEquipment(resource.ID, resource.Grade);
            _inventory.GainEquipment(resource.ID, resource.Grade);
        }
    }


    
    // 합성 메인 슬롯에서 장비 제거
    public void RemoveMainEquip(CraftingInfo equip)
    {
        if(_mainCraftSlot != null &&  _mainCraftSlot.IsEquals(equip.InventoryKey))
        {
            _mainCraftSlot = null;
            _resourceCraftSlots = null;
        }
    }

    // 합성 재료 슬롯에서 특정 장비 제거
    public void RemoveResourceEquip(CraftingInfo equip)
    {
        if (_resourceCraftSlots == null)
            return;

        for (int i = _resourceCraftSlots.Length; i > 0; i--)
        {
            var slot = _resourceCraftSlots[i - 1];
            if (slot != null && slot.IsEquals(equip.InventoryKey))
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
            _inventory.RemoveEquipment(slot.InventoryKey);
        }

        _resourceCraftSlots = new CraftingInfo[_mainCraftSlot.GradeData.cost_count];
    }


    
}

public class CraftingInfo
{
    public ItemInstance InventoryKey;
    public readonly int ItemID;
    public GRADE Grad;

    public PART Parts => ItemDB.GetData<DefaultEquipData>(ItemID).Equipmnet.part;
    //public PART Parts => ItemDB.GetItemData<ItemDataLegacySO>().GetEquipData(ItemID).Equipmnet.part;
    public EquipGrade GradeData => ItemDB.GetData<DefaultEquipData>(ItemID).Grades[(int)Grad];
    //public EquipGrade GradeData => ItemDB.GetItemData<ItemDataLegacySO>().GetEquipData(ItemID).Grades[(int)Grad];
    public readonly bool IsLoadout;

    /// <summary>
    /// 인벤토리에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="inventoryKey"></param>
    public CraftingInfo(ItemInstance inventoryKey)
    {
        InventoryKey = inventoryKey;
        ItemID = inventoryKey.ID;
        Grad = inventoryKey.Grade;
        IsLoadout = false;
    }

    /// <summary>
    /// 착용 장비에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="equipment"></param>
    public CraftingInfo(EquipmentBase equipment)
    {
        ItemID = equipment.Data.Equipmnet.id;
        Grad = equipment.GradeData.grade;
        IsLoadout = true;
    }

    public bool IsEquals(ItemInstance item)
    {
        return InventoryKey.ID == item.ID &&
       InventoryKey.EnhanceLevel == item.EnhanceLevel &&
       InventoryKey.Grade == item.Grade;
    }
}