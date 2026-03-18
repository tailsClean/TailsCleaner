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




    public T GetData<T>(int id) where T : ItemDataBase
    {
        if (_itemDataDict == null)
            Init();

        T item = (T)(ItemDataBase)GetEquipData(id);

        if(item == null)
            item = (T)(ItemDataBase)GetRelicData(id);

        if(item == null)
            item = (T)(ItemDataBase)GetItemData(id);

        if(item == null)
            Debug.LogError($"ID: {id} / 타입: {typeof(T)}에 맞는 아이템은 없습니다.");

        return item;
    }







    #region 장비 데이터 매칭

    private Dictionary<int, EquipData> _equipDict;

    public Dictionary<int, EquipData> EquipDict => _equipDict;


    public EquipData GetEquipData(int id)
    {
        if (_equipDict == null)
            EquipInit();

        if (_equipDict.TryGetValue(id, out var equip))
            return equip;

        Debug.LogWarning($"{id}에 해당하는 장비 데이터가 없습니다.");
        return null;
    }


    public void EquipInit()
    {
        _equipDict = new Dictionary<int, EquipData>();

        var nameData = DataManager.Instance.GetSOData<StringSO>();
        // 장비 데이터 추가
        var equipData = DataManager.Instance.GetSOData<EquipitemSO>();
        foreach (var equip in equipData.dataList)
        {
            _equipDict.Add(equip.id, new EquipData());
            _equipDict[equip.id].GroupID = equip.group_id;
            _equipDict[equip.id].Equipmnet = equip;
            _equipDict[equip.id].StringData = nameData.GetById(equip.name);
        }

        // 장비 스텟을 추가
        var equipStatData = DataManager.Instance.GetSOData<EquipStatSO>();
        foreach (var stat in equipStatData.dataList)
        {
            foreach (var equip in _equipDict.Values)
            {
                if (equip.GroupID == stat.group_id)
                {
                    equip.Stat.Add(stat.type, stat);
                    break;
                }
            }
        }

        // 장비 강화 데이터 추가
        var equipEnhanceData = DataManager.Instance.GetSOData<EquipEnhanceSO>();
        foreach (var enhance in equipEnhanceData.dataList)
        {
            foreach (var equip in _equipDict.Values)
            {
                if (equip.GroupID == enhance.group_id)
                {
                    equip.Enhances.Add(enhance);
                    break;
                }
            }
        }

        // 장비 등급 데이터 추가
        var equipGradeData = DataManager.Instance.GetSOData<EquipGradeSO>();
        foreach (var grade in equipGradeData.dataList)
        {
            foreach (var equip in _equipDict.Values)
            {
                if (equip.GroupID == grade.group_id)
                {
                    equip.Grades.Add(grade);
                    break;
                }
            }
        }
    }

    #endregion

    #region 유물 데이터 매칭

    private Dictionary<int, RelicData> _relicDict;

    public Dictionary<int, RelicData> RelicDict => _relicDict;

    public RelicData GetRelicData(int id)
    {
        if (_relicDict == null)
            RelicInit();

        if (_relicDict.TryGetValue(id, out var equip))
            return equip;

        Debug.LogWarning($"{id}에 해당하는 장비 데이터가 없습니다.");
        return null;
    }


    public void RelicInit()
    {
        _relicDict = new Dictionary<int, RelicData>();

        // 유물 데이터 매칭
        var relicData = DataManager.Instance.GetSOData<RelicSO>();
        foreach (var relic in relicData.dataList)
        {
            _relicDict.Add(relic.group_id, new RelicData());
            _relicDict[relic.group_id].GroupID = relic.group_id;
            _relicDict[relic.group_id].Relic = relic;
        }

        // 유물 강화 데이터 매칭
        var relicEnhanceData = DataManager.Instance.GetSOData<RelicEnhanceSO>();
        foreach (var enhance in relicEnhanceData.dataList)
        {
            foreach (var relic in _relicDict.Values)
            {
                if (relic.GroupID == enhance.group_id)
                {
                    relic.Enhances.Add(enhance);
                    break;
                }
            }
        }

        // 유물 계열 데이터 매칭
        var relicDivisionData = DataManager.Instance.GetSOData<RelicDivisionSO>();
        foreach (var relic in _relicDict.Values)
        {
            foreach (var division in relicDivisionData.dataList)
            {
                if (relic.Relic.relic_type == division.division_type)
                {
                    relic.Division = division;
                    break;
                }
            }
        }
    }

    #endregion


    // 재료, 소모품 데이터는 여기서 관리
    #region 아이템 관리 데이터 매칭
    private Dictionary<int, ItemManageData> _itemDict;

    public Dictionary<int, ItemManageData> ItemDict => _itemDict;

    public ItemManageData GetItemData(int ManageID)
    {
        if (_itemDict == null)
            ItemInit();

        if (_itemDict.TryGetValue(ManageID, out var item))
            return item;

        Debug.LogWarning($"{ManageID}에 해당하는 아이템 데이터가 없습니다.");
        return null;
    }

    public void ItemInit()
    {
        _itemDict = new Dictionary<int, ItemManageData>();
        var itemManageData = DataManager.Instance.GetSOData<ItemManageTableSO>();
        var itemConsumeData = DataManager.Instance.GetSOData<ItemConsumeTableSO>();
        foreach (var manage in itemManageData.dataList)
        {
            _itemDict.Add(manage.item_id, new ItemManageData());
            _itemDict[manage.item_id].ManageID = manage.item_id;
            _itemDict[manage.item_id].Type = manage.item_type;
            _itemDict[manage.item_id].ManageData = manage;
        }
        foreach (var consume in itemConsumeData.dataList)
        {
            if (_itemDict.TryGetValue(consume.item_id, out var item))
                item.Consume = consume;
        }
    }

    #endregion





















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



    //public static ItemBaseSO GetItemData(int id) => GetItemData<ItemBaseSO>(id);


    public static T GetData<T>(int id) where T : ItemDataBase
    {
        if(_itemDB == null)
        {
            _itemDB = Resources.Load<ItemDBSO>("Data/ScriptableObjects/Item/ItemDB");
            Debug.Log("<color=green>ItemDB 초기화</color>");
        }

        return _itemDB.GetData<T>(id);
    }


    //public static T GetItemData<T>(int id = 0) where T : ItemBaseSO
    //{
    //    if(_itemDB == null)
    //    {
    //        _itemDB = Resources.Load<ItemDBSO>("Data/ScriptableObjects/Item/ItemDB");
    //        Debug.Log("<color=green>ItemDB 초기화</color>");
    //    }

    //    return _itemDB.GetValue<T>(id);
    //}

    public static T CreateItem<T>(int id) where T: ItemBase, new()
    {
        var item = new T();
        item.Init(id);

        return item;
    }
}

public class ItemDataBase
{
    public String StringData;
    public Sprite SpriteImg;

    public virtual ITEM_TYPE Type { get; set; }
}