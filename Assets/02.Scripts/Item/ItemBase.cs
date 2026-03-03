using UnityEngine;


public abstract class ItemBase : MonoBehaviour
{
    [Header("아이템 기본 정보")]
    public bool IsItem;
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public ITEM_TYPE ItemType { get; private set; }
    [field: SerializeField] public int MaxStack { get; private set; } = 1;
    [field: SerializeField] public int ItemNameKey { get; private set; }



}

public enum ITEM_TYPE
{
    System, Equipment, Relic, Reinforcement, Consume
}