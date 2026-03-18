using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDB", menuName = "ItemDB")]
public class ItemDBSO : ScriptableObject
{

    #region 기본 장비 데이터 매칭

    private Dictionary<int, DefaultEquipData> _defaultEquipDict;

    public Dictionary<int, DefaultEquipData> DefaultEquipDict => _defaultEquipDict;


    public DefaultEquipData GetDefaultEquipData(int id)
    {
        if (_defaultEquipDict == null)
            DefaultEquipInit();

        if (_defaultEquipDict.TryGetValue(id, out var equip))
            return equip;

        Debug.LogWarning($"{id}에 해당하는 장비 데이터가 없습니다.");
        return null;
    }


    public void DefaultEquipInit()
    {
        _defaultEquipDict = new Dictionary<int, DefaultEquipData>();

        var nameData = DataManager.Instance.GetSOData<StringSO>();
        var equipData = DataManager.Instance.GetSOData<EquipitemSO>();

        foreach (var equip in equipData.dataList)
        {
            _defaultEquipDict.Add(equip.id, new DefaultEquipData());
            _defaultEquipDict[equip.id].UniqueID = equip.id;
            _defaultEquipDict[equip.id].GroupID = equip.group_id;
            _defaultEquipDict[equip.id].Equipmnet = equip;
            _defaultEquipDict[equip.id].StringData = nameData.GetById(equip.name);
            //_equipDict[equip.id].SpriteImg = Resources.Load<Sprite>($"All_Resource/image/Total_Item_Image/{equip.sprite}");
            _defaultEquipDict[equip.id].SpriteImg = Resources.Load<Sprite>($"Total_Item_Image/{equip.sprite}");
        }

        // 장비 스텟을 추가
        var equipStatData = DataManager.Instance.GetSOData<EquipStatSO>();
        foreach (var stat in equipStatData.dataList)
        {
            foreach (var equip in _defaultEquipDict.Values)
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
            foreach (var equip in _defaultEquipDict.Values)
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
            foreach (var equip in _defaultEquipDict.Values)
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

    #region 재료 장비 데이터 매칭


    private Dictionary<int, MaterialEquipData> _materialEquipDict;

    public MaterialEquipData GetMaterialEquipData(int id)
    {
        if (_materialEquipDict == null)
            MaterialEquipInit();

        if (_materialEquipDict.TryGetValue(id, out var equip))
            return equip;

        Debug.LogWarning($"{id}에 해당하는 재료 장비 데이터가 없습니다.");
        return null;
    }

    public void MaterialEquipInit()
    {
        _materialEquipDict = new Dictionary<int, MaterialEquipData>();

        var nameData = DataManager.Instance.GetSOData<StringSO>();
        var equipment = DataManager.Instance.GetSOData<EquipMatterSO>();

        foreach (var equip in equipment.dataList)
        {
            _materialEquipDict.Add(equip.id, new MaterialEquipData());
            _materialEquipDict[equip.id].UniqueID = equip.id;
            _materialEquipDict[equip.id].EquipMatter = equip;
            _materialEquipDict[equip.id].StringData = nameData.GetById(equip.name);
            _materialEquipDict[equip.id].SpriteImg = Resources.Load<Sprite>($"Total_Item_Image/{equip.sprite}");
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
            _relicDict.Add(relic.id, new RelicData());
            _relicDict[relic.id].UniqueID = relic.id;
            _relicDict[relic.id].GroupID = relic.group_id;
            _relicDict[relic.id].Relic = relic;
            _relicDict[relic.id].SpriteImg = Resources.Load<Sprite>($"Total_Item_Image/{relic.sprite}");
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
            _itemDict[manage.item_id].UniqueID = manage.item_id;
            _itemDict[manage.item_id].Type = manage.item_type;
            _itemDict[manage.item_id].ManageData = manage;
            _itemDict[manage.item_id].SpriteImg = Resources.Load<Sprite>($"Total_Item_Image/{manage.item_img}");
        }
        foreach (var consume in itemConsumeData.dataList)
        {
            if (_itemDict.TryGetValue(consume.item_id, out var item))
                item.Consume = consume;
        }
    }


    #endregion



    public T GetData<T>(int id) where T : ItemDataBase
    {

        T item = (T)(ItemDataBase)GetDefaultEquipData(id);

        if (item == null)
            item = (T)(ItemDataBase)GetMaterialEquipData(id);

        if (item == null)
            item = (T)(ItemDataBase)GetRelicData(id);

        if (item == null)
            item = (T)(ItemDataBase)GetItemData(id);

        if (item == null)
            Debug.LogError($"ID: {id} / 타입: {typeof(T)}에 맞는 아이템은 없습니다.");

        return item;
    }
}


// 전역 참조
public static class ItemDB
{
    private static ItemDBSO _itemDB;


    public static T GetData<T>(int id) where T : ItemDataBase
    {
        if(_itemDB == null)
        {
            _itemDB = Resources.Load<ItemDBSO>("Data/ScriptableObjects/Item/ItemDB");
            Debug.Log("<color=green>ItemDB 초기화</color>");
        }

        return _itemDB.GetData<T>(id);
    }

    public static T CreateItem<T>(int id) where T: ItemBase, new()
    {
        var item = new T();
        item.Init(id);

        return item;
    }
}

public class ItemDataBase
{
    public int UniqueID;
    public String StringData;
    public Sprite SpriteImg;

    public virtual ITEM_TYPE Type { get; set; }
}


public enum ITEM_TYPE
{
    System, Equipment, Relic, Reinforcement, Consume
}