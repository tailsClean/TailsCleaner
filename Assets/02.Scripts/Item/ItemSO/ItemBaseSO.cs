using UnityEngine;


public abstract class ItemBaseSO : ScriptableObject
{
    //
    [Header("테스트용 스프라이트")]
    [SerializeField] Sprite TestSprite;
    //


    [Header("아이템 관리용 정보")]
    [SerializeField] protected int _uniqueID;
    [SerializeField] protected ITEM_TYPE _itemType;
    [SerializeField] protected int _maxStack = 1;
    [SerializeField] protected int _itemNameKey;
    [SerializeField] protected string _imageSprite;
    [SerializeField] protected int _maxQuantity;


    public int UniqueID => _uniqueID;
    public ITEM_TYPE ItemType => _itemType;
    public int MaxStack => _maxStack;
    public int ItemNameKey => _itemNameKey;
    public Sprite ImageSprite => TestSprite;

}

public enum ITEM_TYPE
{
    System, Equipment, Relic, Reinforcement, Consume
}