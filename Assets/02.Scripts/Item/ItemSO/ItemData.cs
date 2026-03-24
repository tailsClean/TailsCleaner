using System;
using System.Collections.Generic;
using UnityEngine;

// 장착 장비 데이터
public class DefaultEquipData : ItemDataBase
{
    public int GroupID;
    public Equipitem Equipmnet;
    public Dictionary<EQUIP_STAT_TYPE, EquipStat> Stat;
    public List<EquipGrade> Grades;
    private List<EquipEnhance> _enhances;                    // 메서드로 접근
    private List<Sprite> _sprites;

    public override ITEM_TYPE Type => ITEM_TYPE.Equipment;


    public DefaultEquipData()
    {
        Stat = new Dictionary<EQUIP_STAT_TYPE, EquipStat>();
        Grades = new List<EquipGrade>();
        _enhances = new List<EquipEnhance>();
        _sprites = new List<Sprite>();
    }

    public EquipEnhance GetEnhance(int enhanceLevel)
    {
        if (enhanceLevel == 0)
            return null;

        if (enhanceLevel < 1 || enhanceLevel > _enhances.Count)
        { Debug.LogError($"{Equipmnet.id}의 강화레벨 조회 불가"); return null; }

        return _enhances[enhanceLevel - 1];
    }

    public void SetEnhance(EquipEnhance enhance) => _enhances.Add(enhance);

    public void SetEquipSprite(Sprite sprite) => _sprites.Add(sprite);
    public Sprite GetEquipSprite(GRADE grade) => _sprites[(int)grade];
}

// 재료 장비 데이터
public class MaterialEquipData : ItemDataBase
{
    public int EquipID;
    public EquipMatter EquipMatter;

    public override ITEM_TYPE Type => ITEM_TYPE.Equipment;
}

// 유물 데이터
public class RelicData : ItemDataBase
{
    public int GroupID;
    public Relic Relic;
    public RelicDivision Division;
    private List<RelicEnhance> _enhances;                   // 메서드로 접근

    public override ITEM_TYPE Type => ITEM_TYPE.Relic;

    public RelicData()
    {
        _enhances = new List<RelicEnhance>();
    }

    public RelicEnhance GetEnhance(int enhanceLevel)
    {
        if (enhanceLevel == 0)
            return null;

        if (enhanceLevel < 1 || enhanceLevel > _enhances.Count)
        { Debug.LogError($"{Relic.id}의 강화레벨 조회 불가"); return null; }

        return _enhances[enhanceLevel - 1];
    }

    public void SetEnhance(RelicEnhance relic) => _enhances.Add(relic);
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