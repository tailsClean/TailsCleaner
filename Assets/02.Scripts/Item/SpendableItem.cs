using UnityEngine;


public class SpendableItem : ItemBase
{
    [field: SerializeField] public STAT_TYPE Type {  get; private set; }
    [field: SerializeField] public int OptValue { get; private set; }
    [field: SerializeField] public string OptDescription { get; private set; }
    [field: SerializeField] public string Script { get; private set; }
    [field: SerializeField] public string UsingAfterIllust { get; private set; }

    public void UseItem()
    {

    }

    public enum STAT_TYPE
    {

    }
}
