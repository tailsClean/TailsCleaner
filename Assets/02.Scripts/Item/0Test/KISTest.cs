using UnityEngine;

#if UNITY_EDITOR
public class KISTest : MonoBehaviour
{
    public Inventory inventory;
    public CraftingSystem craftingSystem;
    public int Id;
    public int Amount;



    [ContextMenu("강화 재료 재충전/")]
    private void SetItem()
    {
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.WeaponReinforceResource, Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.HatReinforceResource   , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.CloakReinforceResource , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.ShoseReinforceResource , Amount);
    }

    [ContextMenu("유물 획득/")]
    private void Set()
    {
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.RelicReinforceResource, Amount);
        inventory.SetRelic(new RelicStatus(0, 50, 0));
        inventory.SetRelic(new RelicStatus(1, 60, 0));
        inventory.SetRelic(new RelicStatus(2, 70, 0));

    }

    [ContextMenu("합성 장비 획득")]

    public void GainEquip()
    {
        inventory.GainEquipment(1000, EQUIP_GRADE.Grimy);
        inventory.GainEquipment(2000, EQUIP_GRADE.Grimy);
        inventory.GainEquipment(3000, EQUIP_GRADE.Grimy);

        inventory.GainEquipment(2000, EQUIP_GRADE.Fresh);
        inventory.GainEquipment(3000, EQUIP_GRADE.Fresh);
    }

    [ContextMenu("합성 장비 세팅")]
    public void SetSlot()
    {
        craftingSystem.SetCraftSlot(inventory.GetCrafting(1000, EQUIP_GRADE.Grimy));
        craftingSystem.SetCraftSlot(inventory.GetCrafting(2000, EQUIP_GRADE.Fresh));
        craftingSystem.SetCraftSlot(inventory.GetCrafting(3000, EQUIP_GRADE.Fresh));
    }

    [ContextMenu("장비 재세팅")]
    public void ReSetSlot()
    {
        foreach(var equip in inventory.EquipStates)
        {
            Debug.Log($"ID: {equip.UniqueID}, 등급: {equip.Grade}");
        }

        craftingSystem.SetCraftSlot(inventory.GetCrafting(2000, EQUIP_GRADE.Grimy));
        craftingSystem.SetCraftSlot(inventory.GetCrafting(3000, EQUIP_GRADE.Grimy));
    }

    [ContextMenu("합성")]
    public void Crafting()
    {
        craftingSystem.OnStartCrafting();

        foreach (var equip in inventory.EquipStates)
        {
            Debug.Log($"ID: {equip.UniqueID}, 등급: {equip.Grade}");
        }
    }
}
#endif
