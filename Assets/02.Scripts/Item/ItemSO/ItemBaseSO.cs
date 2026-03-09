using UnityEngine;


public abstract class ItemBaseSO : ScriptableObject
{
    //
    [Header("테스트용 스프라이트")]
    [SerializeField] Sprite TestSprite;
    //


    [Header("아이템 관리용 정보")]
    [SerializeField] private int _uniqueID;
    [SerializeField] private ITEM_TYPE _itemType;
    [SerializeField] private int _maxStack = 1;
    [SerializeField] private int _itemNameKey;
    [SerializeField] private string _imageSprite;
    [SerializeField] private int _maxQuantity;


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