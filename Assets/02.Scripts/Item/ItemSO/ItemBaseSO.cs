using UnityEngine;


public abstract class ItemBaseSO : ScriptableObject
{
    [Header("아이템 기본 정보")]
    [SerializeField] private int _itemID;
    [SerializeField] private ITEM_TYPE _itemType;
    [SerializeField] private int _maxStack = 1;
    [SerializeField] private int _itemNameKey;
    [SerializeField] private string _imageSprite;


    public int ItemID => _itemID;
    public ITEM_TYPE ItemType => _itemType;
    public int MaxStack => _maxStack;
    public int ItemNameKey => _itemNameKey;
    public string ImageSprite => _imageSprite;

}

public enum ITEM_TYPE
{
    System, Equipment, Relic, Reinforcement, Consume
}