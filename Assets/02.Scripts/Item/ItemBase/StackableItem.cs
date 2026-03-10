using UnityEngine;


public class StackableItem : ItemBase
{
    public StackableItemSO Data { get; private set; }

    public override void Init(int id) => Data = ItemDB.GetItemData<StackableItemSO>(id);
}