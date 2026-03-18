using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDBTest : MonoBehaviour
{
    public SpriteRenderer[] Sprites;

    public ItemDBSO itemDB;

    [ContextMenu("아이템 호출")]
    public void Sterr()
    {
        var equip = itemDB;
        foreach (var a in equip.DefaultEquipDict)
        {
            Debug.Log("========");
            Debug.Log("ID: " + a.Key);
            foreach(var d in a.Value.Stat)
                Debug.LogWarning("스탯정보" + d.Value.type);
            Debug.LogWarning(DataManager.Instance.GetSOData<StringSO>().GetById(a.Value.Equipmnet.name).kr);
            Debug.Log("이름: " + a.Value.Equipmnet.name);
            foreach (var b in a.Value.Enhances)
                Debug.Log("강화정보" + b.add_value);
            foreach(var c in a.Value.Grades)
                Debug.Log("세트정보" + c.grade);
            Debug.Log("========");
        }
    }
    
    [ContextMenu("유물 호출")]
    public void Steedrr()
    {
        var equip = itemDB;
        equip.RelicInit();

        foreach (var a in equip.RelicDict)
        {
            Debug.Log("========");
            Debug.Log("ID: " + a.Key);
            Debug.LogWarning(DataManager.Instance.GetSOData<StringSO>().GetById(a.Value.Relic.name).kr);
            foreach (var b in a.Value.Enhances)
                Debug.Log("강화정보" + b.add_value);
            Debug.Log("세트정보" + a.Value.Division);
            Debug.Log("세트정보" + a.Value.Division.name);
            Debug.Log("========");
        }
    }

    [ContextMenu("특정 아이템 정보 호출")]
    public void Sterrsef()
    {
        var equip = itemDB;
        equip.ItemInit();

        foreach(var data in equip.ItemDict.Values)
        {
            Debug.Log("========");
            Debug.Log("ID: " + data.UniqueID);
            Debug.Log("타입: " + data.Type);
            Debug.LogWarning(DataManager.Instance.GetSOData<StringSO>().GetById(data.ManageData.item_name_key.ToString()).kr);
            Debug.Log(data.Consume);
                Debug.Log("========");

        }
    }

    [ContextMenu("DB호출")]
    public void Sterrded()
    {
        var A = ItemDB.GetData<DefaultEquipData>(ItemID.DefaultWeapon);
        var B = ItemDB.GetData<ItemManageData>(ItemID.RelicReinforceResource);

        Debug.Log(A.Equipmnet.name);
        Debug.Log(B.ManageData.item_name_key);
    }

    [ContextMenu("DB호출2")]
    public void asdf()
    {
        itemDB.DefaultEquipInit();
        itemDB.RelicInit();
        itemDB.ItemInit();

        List<ItemDataBase> list = new List<ItemDataBase>();

        var a = itemDB.DefaultEquipDict;
        foreach(var data in a.Values)
        {
            list.Add(data);
        }
        
        for(int i = 0; i < list.Count; i++)
        {
            Sprites[i].sprite = list[i].SpriteImg;
        }
    }    

    [ContextMenu("DB호출3")]
    public void asdfdedd()
    {
        itemDB.DefaultEquipInit();
        itemDB.RelicInit();
        itemDB.ItemInit();

        List<ItemDataBase> list = new List<ItemDataBase>();

        var a = itemDB.ItemDict;
        foreach(var data in a.Values)
        {
            list.Add(data);
        }
        
        for(int i = 0; i < list.Count; i++)
        {
            if(list[i].SpriteImg == null)
                Sprites[i].gameObject.SetActive(false);

            Sprites[i].sprite = list[i].SpriteImg;
        }
    }    

}
