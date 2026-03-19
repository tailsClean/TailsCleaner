using UnityEngine;


public class KISTest : MonoBehaviour
{
    public ItemInventory inventory;
    public Currency currency;
    public CraftingSystem craftingSystem;
    public int Id;
    public int Amount;
#if UNITY_EDITOR
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
        inventory.GainRelic(33004, 0);
        inventory.GainRelic(33102, 0);
        inventory.GainRelic(33201, 0);

    }

    [ContextMenu("합성 장비 획득")]

    public void GainEquip()
    {
        inventory.GainEquipment(1000,GRADE.Dirty);
        inventory.GainEquipment(2000,GRADE.Dirty);
        inventory.GainEquipment(3000,GRADE.Dirty);


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

    [ContextMenu("장착 유물 확인")]
    public void GetRelic()
    {
        var relics = ItemManager.Instance.Loadout.MyRelics;
        if(relics.Count == 0)
        {
            Debug.Log("장착된 유물이 없습니다.");
            return;
        }

        foreach(var relic in relics)
        {
            Debug.Log($"ID: {relic.Data.Name} / 강화레벨: {relic.EnhanceLevel}");
        }
    }

    [ContextMenu("재료 장비 확인")]
    public void SDFE()
    {
        inventory.GainEquipment(39002, GRADE.Normal, 2);
        inventory.GainEquipment(39102, GRADE.Normal, 2);
        inventory.GainEquipment(39201, GRADE.Dirty, 2);
    }


    [ContextMenu("골드 추가 및 소모")]
    public void AASD()
    {
        currency.GainGold(100);
        Debug.Log("골드 " + currency.GoldAmount);
    }

#endif

}
