using UnityEngine;

#if UNITY_EDITOR
public class KISTest : MonoBehaviour
{
    public ItemInventory inventory;
    public CraftingSystem craftingSystem;
    public int Id;
    public int Amount;



    [ContextMenu("강화 재료 재충전")]
    private void SetItem()
    {
        inventory.GainStackItem(ItemID.WeaponReinforceResource, Amount);
        inventory.GainStackItem(ItemID.HatReinforceResource   , Amount);
        inventory.GainStackItem(ItemID.CloakReinforceResource , Amount);
        inventory.GainStackItem(ItemID.ShoseReinforceResource , Amount);
    }

    [ContextMenu("유물 획득")]
    private void Set()
    {
        inventory.GainStackItem(ItemID.RelicReinforceResource, Amount);
        inventory.GainRelic(50, 0);
        inventory.GainRelic(60, 0);
        inventory.GainRelic(60, 0);

    }

    [ContextMenu("합성 장비 획득")]

    public void GainEquip()
    {
        inventory.GainEquipment(1000, 0 ,EQUIP_GRADE.Grimy);
        inventory.GainEquipment(2000, 0 ,EQUIP_GRADE.Grimy);
        inventory.GainEquipment(3000, 0 ,EQUIP_GRADE.Grimy);


        //inventory.GainEquipment(2000, 0 ,EQUIP_GRADE.Fresh);
        //inventory.GainEquipment(3000, 0 ,EQUIP_GRADE.Fresh);
    }

    [ContextMenu("소모품 획득")]

    public void Consume()
    {
        inventory.GainStackItem(123, Amount);
    }

    [ContextMenu("인벤토리 내용물")]
    public void Get()
    {
        foreach(var item in inventory.Inventory)
        {
            Debug.Log($"ID: {item.Key.ID} / 강화레벨: {item.Key.EnhanceLevel} / 등급: {item.Key.Grade} \n수량: {item.Value}");
        }
    }
}
#endif
