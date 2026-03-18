using System.Collections.Generic;
using UnityEngine;


public class PlayerLoadoutUI : UIGroup
{
    [Header("장비일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutSlots;

    [Header("유물일 경우 세팅하는 슬롯")]
    [SerializeField] private List<UISlot> _loadoutRelicSlots;


    
    private PlayerLoadout _loadout;



    private void OnEnable()
    {
        if (_loadout != null)
            UpdateLoadoutSlot();

        if (_loadout != null)
            UpdateRelicLoadoutSlot();
    }


    protected override void Start()
    {
        base.Start();
        _loadout = ItemManager.Instance.Loadout;

            UpdateLoadoutSlot();

            UpdateRelicLoadoutSlot();
    }


    private void UpdateLoadoutSlot()
    {
        if (_loadoutSlots != null && _loadoutSlots.Count > 0)
        {
            int i = 0;
            foreach (var equip in _loadout.MyEquipments.Values)
            {
                _loadoutSlots[i++].SetSlot(equip.Data.Equipmnet.id);
            }
        }
    }

    private void UpdateRelicLoadoutSlot()
    {

        if(_loadoutRelicSlots != null && _loadoutRelicSlots.Count > 0)
        {
            int i = 0;
            foreach (var relic in _loadout.MyRelics)
            {
                _loadoutRelicSlots[i++].SetSlot(relic.Data.Relic.id);
            }
            for (; i < _loadoutRelicSlots.Count; i++)
            {
                _loadoutRelicSlots[i].Init();
            }
        }

    }
}
