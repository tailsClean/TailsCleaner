using UnityEngine;


public class StackableItem : ItemBase
{
    public ItemManageData Data { get; private set; }

    public override void Init(int id) => Data = ItemDB.GetData<ItemManageData>(id);
}