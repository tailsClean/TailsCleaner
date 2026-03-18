using UnityEngine;


public class ConsumeItem : ItemBase
{
    private ItemManageData Data;

    public override void Init(int id) => Data = ItemDB.GetData<ItemManageData>(id);
}