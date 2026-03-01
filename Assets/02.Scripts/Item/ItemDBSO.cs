using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDB", menuName = "ItemDB")]
public class ItemDBSO : ScriptableObject
{
    [Header("장비")]
    [SerializeField] private List<EquipmentBase> _equipments;

    [Header("유물")]
    [SerializeField] private List<RelicBase> _Relics;

    private Dictionary<int, IDBable> _itemDict;

    // DB에서 특정 ID의 아이템 반환
    public T GetValue<T>(int id) where T : IDBable
    {
        if (_itemDict == null)
            Init();

        if (_itemDict.TryGetValue(id, out var item))
        {
            if (item is T tItem)
                return tItem;
        }
        return default;
    }

    private void Init()
    {
        _itemDict = new Dictionary<int, IDBable>();
        SetItemDict(_equipments);
        SetItemDict(_Relics);
    }

    private void SetItemDict<T>(List<T> items) where T : IDBable
    {
        foreach(var item in items)
        {
            _itemDict.Add(item.ID, item);
        }
    }
}

public interface IDBable
{
    int ID { get; }
}

public class ItemDB
{
    private static ItemDBSO _itemDB;

    public static T GetItem<T>(int id) where T : IDBable
    {
        if(_itemDB == null)
        {
            _itemDB = Resources.Load<ItemDBSO>("SO/Item/ItemDB");
            Debug.Log("초기화");
        }

        return _itemDB.GetValue<T>(id);
    }
}