using System.Collections;
using UnityEngine;


public class KISTest : MonoBehaviour
{
    public ItemInventory inventory;
    public ItemCurrency currency;
    public CraftingSystem craftingSystem;
    public int Id;
    public int Amount = 0;


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
        inventory.GainRelic(33001, 0);
        inventory.GainRelic(33002, 0);
        inventory.GainRelic(33003, 0);
        inventory.GainRelic(33004, 0);
        inventory.GainRelic(33005, 0);
        inventory.GainRelic(33006, 0);
        inventory.GainRelic(33101, 0);
        inventory.GainRelic(33102, 0);  
        inventory.GainRelic(33103, 0);
        inventory.GainRelic(33104, 0);
        inventory.GainRelic(33105, 0);
        inventory.GainRelic(33106, 0);
        inventory.GainRelic(33201, 0);
        inventory.GainRelic(33202, 0);
        inventory.GainRelic(33203, 0);
        inventory.GainRelic(33204, 0);
        inventory.GainRelic(33205, 0);
        inventory.GainRelic(33206, 0);
        inventory.GainRelic(33301, 0);
        inventory.GainRelic(33302, 0);
        inventory.GainRelic(33303, 0);
        inventory.GainRelic(33304, 0);
        inventory.GainRelic(33305, 0);
        inventory.GainRelic(33306, 0);
    }

    [ContextMenu("합성 장비 획득")]

    public void GainEquip()
    {
        //inventory.GainEquipment(1000, GRADE.Dirty);
        //inventory.GainEquipment(2000, GRADE.Dirty);
        //inventory.GainEquipment(3000, GRADE.Dirty);


        //inventory.GainEquipment(2000, 0, GRADE.);
        //inventory.GainEquipment(3000, 0, GRADE.Fresh);
    }

    [ContextMenu("소모품 획득")]

    public void Consume()
    {
        inventory.GainStackItem(34005, 100);
        inventory.GainStackItem(35001, 10);
        //inventory.GainStackItem(35002, 10);
        inventory.GainStackItem(35003, 10);
    }


    [ContextMenu("재료 장비 확인")]
    public void SDFE()
    {
        inventory.GainEquipment(32001, 15);
        inventory.GainEquipment(32002, 15);
        inventory.GainEquipment(32003, 15);
        inventory.GainEquipment(32004, 15);

        inventory.GainEquipment(32101, 15);
        inventory.GainEquipment(32102, 15);
        inventory.GainEquipment(32103, 15);
        inventory.GainEquipment(32104, 15);

        inventory.GainEquipment(32201, 15);
        inventory.GainEquipment(32202, 15);
        inventory.GainEquipment(32203, 15);
        inventory.GainEquipment(32204, 15);

        inventory.GainEquipment(32301, 15);
        inventory.GainEquipment(32302, 15);
        inventory.GainEquipment(32303, 15);
        inventory.GainEquipment(32304, 15);
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
        var relics = PlayerStatManager.Instance.Loadout.MyRelics;
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
        currency.GainGold(0 + Amount);
    }

}
