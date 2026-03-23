using System;
using System.Collections.Generic;
using UnityEngine;

// 장착 장비 데이터
public class DefaultEquipData : ItemDataBase
{
    public int GroupID;
    public Equipitem Equipmnet;
    public Dictionary<EQUIP_STAT_TYPE, EquipStat> Stat;
    public List<EquipEnhance> Enhances;
    public List<EquipGrade> Grades;

    public bool IsLoadoutable => true;
    public override ITEM_TYPE Type => ITEM_TYPE.Equipment;

    public DefaultEquipData()
    {
        Stat = new Dictionary<EQUIP_STAT_TYPE, EquipStat>();
        Enhances = new List<EquipEnhance>();
        Grades = new List<EquipGrade>();
    }
}

// 재료 장비 데이터
public class MaterialEquipData : ItemDataBase
{
    public int EquipID;
    public EquipMatter EquipMatter;

    public bool IsLoadoutable => false;
    public override ITEM_TYPE Type => ITEM_TYPE.Equipment;
}

// 유물 데이터
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

// 기타 아이템 데이터
public class ItemManageData : ItemDataBase
{
    public ItemManageTable ManageData;
    public ItemConsumeTable Consume;

    public override ITEM_TYPE Type { get; set; }

}


public enum Relic_CONDITION
{
    // 값 미지정
}