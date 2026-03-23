#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            enhanceLvl = equip.EnhanceLevel;
            maxlvl = equip.EnhanceData.is_max_level;
            grade = equip.Grade;
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
            enchantLevel = relic.EnhanceLevel;
            maxlevel = relic.EnhanceData.is_max_level;
        }
    }    
}
#endif