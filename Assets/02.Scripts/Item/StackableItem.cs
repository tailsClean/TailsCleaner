using UnityEngine;


public class StackableItem : ItemBase
{
    public Sprite MySprite;




    public override Sprite GetSprite() => MySprite;
}