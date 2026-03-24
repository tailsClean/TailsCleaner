using System.Collections;
using UnityEngine;


public class KISTest : MonoBehaviour
{
    public ItemInventory inventory;
    public ItemCurrency currency;
    public CraftingSystem craftingSystem;
    public int Id;
    public int Amount = 10000000;


    private void Start()
    {
        StartCoroutine(CreateItem());
    }



    private IEnumerator CreateItem()
    {
        yield return null;
        yield return null;

        SetItem();
        Set();
        GainEquip();
        Consume();
        SDFE();
        AASD();
    }


    [ContextMenu("강화 재료 재충전")]
    private void SetItem()
    {
        inventory.GainStackItem(ItemID.WeaponReinforceResource, 1000);
        inventory.GainStackItem(ItemID.HatReinforceResource   , 1000);
        inventory.GainStackItem(ItemID.CloakReinforceResource , 1000);
        inventory.GainStackItem(ItemID.ShoseReinforceResource , 1000);
        inventory.GainStackItem(ItemID.RelicReinforceResource, 1000);
    }

    [ContextMenu("유물 획득")]
    private void Set()
    {
        inventory.GainRelic(33001, 1);
        inventory.GainRelic(33002, 1);
        //inventory.GainRelic(33003, 1);
        //inventory.GainRelic(33004, 1);
        //inventory.GainRelic(33005, 1);
        //inventory.GainRelic(33006, 1);

    }

    [ContextMenu("합성 장비 획득")]

    public void GainEquip()
    {
        //inventory.GainEquipment(1000,GRADE.Dirty);
        //inventory.GainEquipment(2000,GRADE.Dirty);
        //inventory.GainEquipment(3000,GRADE.Dirty);


        //inventory.GainEquipment(2000, 0 ,EQUIP_GRADE.Fresh);
        //inventory.GainEquipment(3000, 0 ,EQUIP_GRADE.Fresh);
    }

    [ContextMenu("소모품 획득")]

    public void Consume()
    {
        //inventory.GainStackItem(34005, 10);
        //inventory.GainStackItem(35001, 10);
        //inventory.GainStackItem(35002, 10);
    }


    [ContextMenu("재료 장비 확인")]
    public void SDFE()
    {
        //inventory.GainEquipment(39001, GRADE.Dirty, 15);
        //inventory.GainEquipment(39002, GRADE.Normal, 15);
        //inventory.GainEquipment(39101, GRADE.Dirty, 15);
        //inventory.GainEquipment(39102, GRADE.Normal, 15);
        //inventory.GainEquipment(39201, GRADE.Dirty, 15);
        //inventory.GainEquipment(39202, GRADE.Dirty, 15);
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
            Debug.Log($"ID: {relic.Data.Name} / 강화레벨: {relic.CurrentEnhanceLevel}");
        }
    }



    [ContextMenu("골드 추가 및 소모")]
    public void AASD()
    {
        currency.GainGold(100 + Amount);
        Debug.Log("골드 " + currency.GoldAmount);
    }

}
