using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDB", menuName = "ItemDB")]
public class ItemDBSO : ScriptableObject
{
    [Header("장비")]
    [SerializeField] private List<ItemBase> _equipments;

    [Header("유물")]
    [SerializeField] private List<ItemBase> _Relics;

    private Dictionary<int, ItemBase> _itemDataDict;              // 게임 아이템 요소 관리 데이터


    // DB에서 특정 ID의 아이템 반환
    public T GetValue<T>(int id) where T : ItemBase
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
        _itemDataDict = new Dictionary<int, ItemBase>();
        Debug.Log("데이터베이스 new할당");
        SetItemDict(_equipments);
        SetItemDict(_Relics);
    }
    private void SetItemDict<T>(List<T> items) where T : ItemBase
    {
        foreach(var item in items)
        {
            _itemDataDict.Add(item.ID, item);
        }
    }
}


// 전역 참조
public static class ItemDB
{
    private static ItemDBSO _itemDB;

    public static T GetItem<T>(int id) where T : ItemBase
    {
        if(_itemDB == null)
        {
            _itemDB = Resources.Load<ItemDBSO>("SO/Item/ItemDB");
            Debug.Log("<color=green>ItemDB 초기화</color>");
        }

        return _itemDB.GetValue<T>(id);
    }
}