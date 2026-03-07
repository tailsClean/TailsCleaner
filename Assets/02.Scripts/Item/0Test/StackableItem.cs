using UnityEngine;


[CreateAssetMenu(fileName = "ItemData", menuName = "ItemData/StackableItem")]
public class StackableItem : ItemBase
{
    public Sprite MySprite;




    public override Sprite GetSprite() => MySprite;
}