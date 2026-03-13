using UnityEngine;


public class ConsumeItem : ItemBase
{
    private ConsumeItemSO Data;

    public override void Init(int id) => Data = ItemDB.GetItemData<ConsumeItemSO>(id);
}