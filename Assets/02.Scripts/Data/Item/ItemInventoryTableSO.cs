using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemInventoryTableSO", menuName = "Data/ItemInventoryTableSO")]
public class ItemInventoryTableSO : ScriptableObject
{
    public List<ItemInventoryTable> dataList = new List<ItemInventoryTable>();

    public ItemInventoryTable GetById(int id)
        => dataList.Find(x => x.inven_id == id);
}
