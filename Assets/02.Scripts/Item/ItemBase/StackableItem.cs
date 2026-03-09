using UnityEngine;


public class StackableItem : ItemBase
{
    private StackableItemSO Data;

    public override void Init(int id) => Data = ItemDB.GetItemSO<StackableItemSO>(id);
}