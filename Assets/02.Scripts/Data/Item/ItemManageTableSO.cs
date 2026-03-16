using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemManageTableSO", menuName = "Data/ItemManageTableSO")]
public class ItemManageTableSO : ScriptableObject
{
    public List<ItemManageTable> dataList = new List<ItemManageTable>();

    public ItemManageTable GetById(int id)
        => dataList.Find(x => x.item_id == id);
}
