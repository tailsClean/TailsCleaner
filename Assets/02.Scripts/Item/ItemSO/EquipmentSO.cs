using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EquipmentSO", menuName = "ItemData/Equipment")]
public class EquipmentSO : ItemBaseSO
{


    private Dictionary<int, EquipData> _equipDict;

    public Dictionary<int, EquipData> EquipDict => _equipDict;


    public EquipData GetEquipData(int id)
    {
        if (_equipDict == null)
            Init();

        if(_equipDict.TryGetValue(id, out var equip))
            return equip;

        Debug.LogWarning($"{id}에 해당하는 장비 데이터가 없습니다.");
        return null;
    }


    public void Init()
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
        foreach(var stat in equipStatData.dataList)
        {
            foreach(var equip in _equipDict.Values)
            {
                if(equip.GroupID == stat.group_id)
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
            foreach(var equip in _equipDict.Values)
            {
                if(equip.GroupID == enhance.group_id)
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
}

public class EquipData
    {
        public int GroupID;
        public Equipitem Equipmnet;
        public Dictionary<EQUIP_STAT_TYPE, EquipStat> Stat;
        public List<EquipEnhance> Enhances;
        public List<EquipGrade> Grades;

        public EquipData()
        {
            Stat = new Dictionary<EQUIP_STAT_TYPE, EquipStat>();
            Enhances = new List<EquipEnhance>();
            Grades = new List<EquipGrade>();
        }
    }


