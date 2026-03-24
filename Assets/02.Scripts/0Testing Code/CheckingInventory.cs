#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 인벤토리 내용물 실시간 확인용 코드
/// </summary>
public class CheckingInventory : MonoBehaviour
{
    public ItemInventory inventory;
    public ItemCurrency currency;

    [Header("보기 전용(수정해도 의미 없음")]
    public int GoldAmount;
    public List<ItemData> itemDatas = new List<ItemData>();


    private void Start()
    {
        if (ItemManager.Instance != null)
        {
            inventory = ItemManager.Instance.Inventory;
            currency = ItemManager.Instance.Currency;
        }
    }

    private void Update()
    {
        GoldAmount = currency.GoldAmount;
        itemDatas.Clear();
        int i = 0;
        foreach(var item in inventory.Inventory)
        {
            itemDatas.Add(new ItemData());
            itemDatas[i++].Set(item.Key, item.Value);
        }
    }

    [Serializable]
    public class ItemData
    {
        public int ID;
        public string Name;
        public int EnhanceLevel;
        public GRADE Grade;
        public ITEM_TYPE Type;
        public int Amount;

        public void Set(ItemInstance item, int amount)
        {
            ID = item.ID;
            Name = item.Name;
            EnhanceLevel = item.EnhanceLevel;
            Grade = item.Grade;

            Type = item.ItemType;
            Amount = amount;
        }
    }
}
#endif