using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/SpendableItem")]
public class SpendableItem : ItemBase
{
    [field: SerializeField] public STAT_TYPE Type {  get; private set; }
    [field: SerializeField] public int OptValue { get; private set; }
    [field: SerializeField] public string OptDescription { get; private set; }
    [field: SerializeField] public string Script { get; private set; }
    [field: SerializeField] public string UsingAfterIllust { get; private set; }

    //
    [SerializeField] private Sprite _sprite;
    //

    public void UseItem()
    {

    }

    public override Sprite GetSprite() => _sprite;

    public enum STAT_TYPE
    {
        Enter, Exp, EnergyUp
    }
}
