using System;
using System.Linq;
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
        _loadout = PlayerStatManager.Instance.Loadout;
    }


    // 합성 장비(함성할 장비, 합성 재료 장비)를 세팅
    public void SetCraftSlot(CraftingInfo equipment, bool isRemoveInventory = true)
    {

        if(_mainCraftSlot == null)
        {
            SetMainCraftSlot(equipment, isRemoveInventory);
            return;
        }

        // 최대 등급 확인
        if (equipment.IsMaxGrade)
        { WarningText.ShowText("최대 등급의 장비입니다."); return; }

        if (equipment.IsLoadout)
        { WarningText.ShowText("<color=yellow>착용장비는 재료로 사용할 수 없습니다.</color>"); return; }

        SetResourceCraftSlots(equipment);
    }

    // 업그레이드 장비칸에 장비 세팅
    private void SetMainCraftSlot(CraftingInfo mainEquip, bool isRemoveInventory)
    {
        if(mainEquip == null)
        { Debug.LogError("설정하려는 아이템이 없습니다."); return; }

        _mainCraftSlot = mainEquip;
        _resourceCraftSlots = new CraftingInfo[mainEquip.CostCount];

        if (!mainEquip.IsLoadout && isRemoveInventory)
            _inventory.RemoveEquipment(mainEquip.InventoryKey);

        OnSetEquipment?.Invoke();
    }

    // 재료 장비칸에 장비 세팅
    private void SetResourceCraftSlots(CraftingInfo resourceEquip)
    {
        if (_mainCraftSlot == null || _resourceCraftSlots == null)
        { Debug.LogWarning("합성 장비부터 세팅하세요."); return; }

        if(!CheckGrade(resourceEquip))
        { WarningText.ShowText("<color=yellow>재료 장비의 등급이 적합하지 않습니다.</color>");  return; }

        if(!CheckParts(resourceEquip))
        { WarningText.ShowText("<color=yellow>재료 장비의 부위가 다릅니다..</color>");  return; }

        // 재료 장비칸 중 null인 칸에 재료 장비를 추가
        for(int i = 0; i < _resourceCraftSlots.Length; i++)
        {
            CraftingInfo slot = _resourceCraftSlots[i];
            if(slot == null)
            {
                _resourceCraftSlots[i] = resourceEquip;

                if (!resourceEquip.IsLoadout)
                    _inventory.RemoveEquipment(resourceEquip.InventoryKey);
                OnSetEquipment?.Invoke();
                return;
            }
        }
        WarningText.ShowText("<color=yellow>재료 장비칸이 가득 찼습니다.</color>");
    }
    // 재료의 등급 확인
    private bool CheckGrade(CraftingInfo resourceEquip) => _mainCraftSlot.Grade == resourceEquip.Grade;
    // 재료의 부위 확인
    private bool CheckParts(CraftingInfo resourceEquip) => _mainCraftSlot.Parts == resourceEquip.Parts;


    // 합성 시작
    public void OnStartCrafting()
    {
        if(_mainCraftSlot == null)
        { WarningText.ShowText("합성 대상 장비가 없습니다."); return; }    

        // 최대 등급 확인
        if (_mainCraftSlot.IsMaxGrade)
        { WarningText.ShowText("최대 등급의 장비입니다."); return; }

        // 필요 재료 갯수 확인
        if (Array.Exists(_resourceCraftSlots, x => x == null))
        { WarningText.ShowText("재료장비의 개수가 부족합니다."); return;}


        // 착용 장비의 경우
        if(_mainCraftSlot.IsLoadout)
        {
            var parts = _mainCraftSlot.Parts;
            _loadout.MyEquipments[parts].OnUpgrade();

            _mainCraftSlot = null;
            _resourceCraftSlots = null;
            SetCraftSlot(new CraftingInfo(_loadout.MyEquipments[parts]));

        }
        // 인벤토리의 재료 장비의 합성일 경우
        else
        {
            ItemInstance originalEquip = _mainCraftSlot.InventoryKey;
            ItemInstance newEquip = new ItemInstance(originalEquip.ID + 1, originalEquip.EnhanceLevel, originalEquip.Grade + 1);
            
            _mainCraftSlot = null;
            _resourceCraftSlots = null;
            Debug.Log(_resourceCraftSlots);
            SetCraftSlot(new CraftingInfo(newEquip), false);
        }

        OnCrafting?.Invoke(_mainCraftSlot);
        Debug.Log("등급 업그레이드 성공!");
    }


    
    // 합성 메인 슬롯에서 장비 제거
    public void RemoveMainEquip()
    {
        if (_mainCraftSlot != null && _mainCraftSlot.IsEquals(_mainCraftSlot.InventoryKey))
        {
            if (!_mainCraftSlot.IsLoadout)
            {
                var originalEquip = _mainCraftSlot.InventoryKey;
                _inventory.GainEquipment(originalEquip.ID);
            }

            foreach (var slot in _resourceCraftSlots)
            {
                if(slot != null)
                    _inventory.GainEquipment(slot.ItemID);
            }

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
                _inventory.GainEquipment(slot.InventoryKey.ID);
                _resourceCraftSlots[i - 1] = null;
                return;
            }
        }

        Debug.Log("지우려는 아이템이 합성 리스트에 없습니다.");
    }

}

[Serializable]
public class CraftingInfo
{
    public ItemInstance InventoryKey;
    public int ItemID;
    public GRADE Grade;
    public int CostCount;
    public bool IsMaxGrade;
    public readonly PART Parts;
    public readonly bool IsLoadout;

    /// <summary>
    /// 인벤토리에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="inventoryKey"></param>
    public CraftingInfo(ItemInstance inventoryKey)
    {
        InventoryKey = inventoryKey;
        ItemID = inventoryKey.ID;
        Grade = inventoryKey.Grade;
        ItemDB.TryGetData<MaterialEquipData>(ItemID, out var result);
        CostCount = result.EquipMatter.cost_count;
        IsMaxGrade = Grade == GRADE.None - 1;
        Parts = result.EquipMatter.part_type;
        IsLoadout = false;
    }

    /// <summary>
    /// 착용 장비에서 꺼낸 아이템을 사용할 때
    /// </summary>
    /// <param name="equipment"></param>
    public CraftingInfo(EquipmentBase equipment)
    {
        InventoryKey = new ItemInstance(equipment.Data.UniqueID, equipment.EnhanceLevel, equipment.CurrentGrade);
        ItemID = equipment.Data.Equipmnet.id;
        Grade = equipment.CurrentGradeData.grade;
        ItemDB.TryGetData<DefaultEquipData>(ItemID, out var result);
        CostCount = result.Grades[(int)Grade].cost_count;
        IsMaxGrade = Grade == GRADE.None - 1;
        //IsMaxGrade = result.Grades[(int)Grade].is_max_grade;
        Parts = result.Equipmnet.part;
        IsLoadout = true;
    }

    public bool IsEquals(ItemInstance item)
    {
        return InventoryKey.ID == item.ID &&
       InventoryKey.EnhanceLevel == item.EnhanceLevel &&
       InventoryKey.Grade == item.Grade;
    }
}