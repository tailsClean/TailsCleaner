using System.Collections;
using UnityEngine;

public class ItemDBTest : MonoBehaviour
{
    public ItemDataLegacySO equip;

    [ContextMenu("아이템 호출")]
    public void Sterr()
    {
        equip.EquipInit();

        foreach (var a in equip.EquipDict)
        {
            Debug.Log("========");
            Debug.Log("ID: " + a.Key);
            foreach(var d in a.Value.Stat)
                Debug.LogWarning("스탯정보" + d.Value.type);
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
        equip.RelicInit();

        foreach (var a in equip.RelicDict)
        {
            Debug.Log("========");
            Debug.Log("ID: " + a.Key);
            Debug.Log("이름: " + a.Value.Relic.name);
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
        var data = equip.GetEquipData(ItemID.DefaultWeapon);

        Debug.Log("========");
        Debug.Log("ID: " + data.Equipmnet.id);
        foreach (var d in data.Stat)
            Debug.LogWarning("스탯정보" + d.Value.type);
        foreach (var b in data.Enhances)
                Debug.Log("강화정보" + b.add_value);
            foreach(var c in data.Grades)
                Debug.Log("세트정보" + c.grade);
            Debug.Log("========");
    }

}
