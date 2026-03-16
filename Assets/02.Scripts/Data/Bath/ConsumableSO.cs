
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableSO", menuName = "Data/ConsumableSO")]
public class ConsumableSO : ScriptableObject
{
    public List<Consumable> dataList = new List<Consumable>();

    public Consumable GetById(int id)
        => dataList.Find(x => x.consumable_id == id);
}
