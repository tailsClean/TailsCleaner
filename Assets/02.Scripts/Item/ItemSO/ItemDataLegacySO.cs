using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentSO", menuName = "ItemData/Equipment")]
public class ItemDataLegacySO : ItemBaseSO
{

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
        // 장비 데이터 추가
        var equipData = DataManager.Instance.GetSOData<EquipitemSO>();
        foreach (var equip in equipData.dataList)
        {
            _equipDict.Add(equip.id, new EquipData());
            _equipDict[equip.id].GroupID = equip.group_id;
            _equipDict[equip.id].Equipmnet = equip;
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
}

public class EquipData : ItemDataBase
{
    public int GroupID;
    public Equipitem Equipmnet;
    public Dictionary<EQUIP_STAT_TYPE, EquipStat> Stat;
    public List<EquipEnhance> Enhances;
    public List<EquipGrade> Grades;
    public override ITEM_TYPE Type => ITEM_TYPE.Equipment;

    public EquipData()
    {
        Stat = new Dictionary<EQUIP_STAT_TYPE, EquipStat>();
        Enhances = new List<EquipEnhance>();
        Grades = new List<EquipGrade>();
    }
}


public class RelicData : ItemDataBase
{
    public int GroupID;
    public Relic Relic;
    public List<RelicEnhance> Enhances;
    public RelicDivision Division;
    public override ITEM_TYPE Type => ITEM_TYPE.Relic;

    public RelicData()
    {
        Enhances = new List<RelicEnhance>();
    }
}

public class ItemManageData : ItemDataBase
{
    public int ManageID;
    public ItemManageTable ManageData;
    public ItemConsumeTable Consume;

    public override ITEM_TYPE Type { get; set; }

}



//public enum STAT_TYPE
//{
//    GoldGainRate,   // 골드 획득량 증가
//    ItemDropRate,   // 아이템 획득 확률 증가
//    ExpGainRate     // 경험치 획득량 증가
//}

public enum Relic_CONDITION
{
    // 값 미지정
}