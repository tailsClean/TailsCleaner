#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CheckingLoadout;

public class CheckingLoadout : MonoBehaviour
{
    public PlayerLoadout playerLoadout;

    [Header("보기 전용(수정해도 의미 없음")]
    public List<Equip> equips;
    public List<Relic> relics;

    private void Start()
    {
        StartCoroutine(Set());
    }

    private void Update()
    {
        if (playerLoadout == null)
            return;

        int i = 0;
        equips.Clear();
        foreach (var equip in playerLoadout.MyEquipments)
        {
            equips.Add(new Equip(equip.Value));
        }
        
        relics.Clear();
        for(i = 0; i < playerLoadout.MyRelics.Count; i++)
        {
            relics.Add(new Relic(playerLoadout.MyRelics[i]));
        }
        
    }

    private IEnumerator Set()
    {
        yield return null;

        if (playerLoadout == null)
            playerLoadout = ItemManager.Instance.Loadout;

        equips = new();
        relics = new();
    }


    [Serializable]
    public struct Equip
    {
        public int id;
        public string name;
        public PART part;
        public int enhanceLvl;
        public bool maxlvl;
        public GRADE grade;

        public Equip(EquipmentBase equip)
        {
            var data = equip.Data;
            id = data.UniqueID;
            name = data.Name;
            part = data.Equipmnet.part;
            enhanceLvl = equip.CurrentEnhanceLevel;
            if (equip.CurrentEnhanceData != null)
                maxlvl = equip.CurrentEnhanceData.is_max_level;
            else
                maxlvl = false;
            grade = equip.CurrentGrade;
        }
    }

    [Serializable]
    public struct Relic
    {
        public int id;
        public string name;
        public int enchantLevel;
        public bool maxlevel;

        public Relic(RelicBase relic)
        {
            var data = relic.Data;
            id = data.UniqueID;
            name = data.Name;
            enchantLevel = relic.CurrentEnhanceLevel;
            if (relic.CurrentEnhanceData != null)
                maxlevel = relic.CurrentEnhanceData.is_max_level;
            else
                maxlevel = false;
        }
    }    
}
#endif