using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDB", menuName = "ItemDB")]
public class ItemDBSO : ScriptableObject
{
    [Header("장비")]
    [SerializeField] private List<ItemDataLegacySO> _equipments;

    [Header("유물")]
    [SerializeField] private List<RelicLegacySO> _Relics;

    [Header("소모품")]
    [SerializeField] private List<ConsumeItemSO> _consumeItems;

    [Header("재화/강화재료")]
    [SerializeField] private List<StackableItemSO> _stackableItems;

    private Dictionary<int, ItemBaseSO> _itemDataDict;              // 게임 아이템 요소 관리 데이터


    // DB에서 특정 ID의 아이템 반환
    public T GetValue<T>(int id) where T : ItemBaseSO
    {
        if (_itemDataDict == null)
            Init();

        if (_itemDataDict.TryGetValue(id, out var item))
        {
            if (item is T tItem)
                return tItem;
        }

        Debug.LogError($"ID: {id} / 타입: {typeof(T)}에 맞는 아이템은 없습니다.");
        return default;
    }


    // 초기화
    private void Init()
    {
        _itemDataDict = new Dictionary<int, ItemBaseSO>();
        Debug.Log("데이터베이스 new할당");
        SetItemDict(_equipments);
        SetItemDict(_Relics);
        SetItemDict(_consumeItems);
        SetItemDict(_stackableItems);
    }
    private void SetItemDict<T>(List<T> items) where T : ItemBaseSO
    {
        foreach(var item in items)
        {
            if (!_itemDataDict.TryAdd(item.UniqueID, item))
                Debug.LogError($"<color=red>{item.name}의 ID가 중복입력됐습니다.</color>");
        }
    }
}


// 전역 참조
public static class ItemDB
{
    private static ItemDBSO _itemDB;


    public static ItemBaseSO GetItemData(int id) => GetItemData<ItemBaseSO>(id);



    public static T GetItemData<T>(int id = 0) where T : ItemBaseSO
    {
        if(_itemDB == null)
        {
            _itemDB = Resources.Load<ItemDBSO>("Data/ScriptableObjects/Item/ItemDB");
            Debug.Log("<color=green>ItemDB 초기화</color>");
        }

        return _itemDB.GetValue<T>(id);
    }

    public static T CreateItem<T>(int id) where T: ItemBase, new()
    {
        var item = new T();
        item.Init(id);

        return item;
    }
}

//public static class ItemDB
//{
//    private static ItemDBSO _itemDB;


//    //public static ItemBaseSO GetItemData(int id) => GetItemData<ItemBaseSO>(id);



//    public static T GetItemData<T>() where T : ItemBaseSO
//    {
//        if (_itemDB == null)
//        {
//            _itemDB = Resources.Load<ItemDBSO>("Data/ScriptableObjects/Item/ItemDB");
//            Debug.Log("<color=green>ItemDB 초기화</color>");
//        }

//        return _itemDB.GetValue<T>(0);
//    }

//    public static T CreateItem<T>(int id) where T : ItemBase, new()
//    {
//        var item = new T();
//        item.Init(id);

//        return item;
//    }
//}