using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerLoadout
{
    private Dictionary<PART, EquipmentBase> _myEquipments;
    private List<RelicBase> _myRelics;

    public Dictionary<PART, EquipmentBase> MyEquipments => _myEquipments;
    public List<RelicBase> MyRelics => _myRelics;

    public PlayerLoadout()
    {
        _myRelics = new List<RelicBase>(3);
        _myEquipments = new Dictionary<PART, EquipmentBase>
        {
            {PART.Weapon, ItemDataBase.CreateItem<EquipmentBase>(ItemID.DefaultWeapon)},
            {PART.Helmet, ItemDataBase.CreateItem<EquipmentBase>(ItemID.DefaultHat)},
            {PART.Cloak, ItemDataBase.CreateItem<EquipmentBase>(ItemID.DefaultCloak)},
            {PART.Shoes, ItemDataBase.CreateItem<EquipmentBase>(ItemID.DefaultShose)}
        };
    }

    // 장비의 스텟 증가량을 반환
    public float GetIncreaseStat(EQUIP_STAT_TYPE stat)
    {
        if (_myEquipments == null)
            return 0;

        EquipmentBase equipment = null;
        switch (stat)
        {
            case EQUIP_STAT_TYPE.Attack:
                equipment = _myEquipments[PART.Weapon];
                break;

            case EQUIP_STAT_TYPE.CriticalRate:
                equipment = _myEquipments[PART.Helmet];
                break;

            case EQUIP_STAT_TYPE.MaxHP or EQUIP_STAT_TYPE.Defense:
                equipment = _myEquipments[PART.Cloak];
                break;

            case EQUIP_STAT_TYPE.MoveSpeed or EQUIP_STAT_TYPE.Dodge:
                equipment = _myEquipments[PART.Shoes];
                break;

            default:
                return 0;
        }

        return equipment.GetIncreaseStat(stat);
    }

    // 유물리스트의 스텟 증가량을 반환
    public float GetIncreaseStat(RELIC_STAT stat)
    {
        int result = 0;
        foreach (var relic in _myRelics)
        {
            if (relic == null)
                continue;

            if (relic.Data.StatType == stat)
                result += relic.Data.GetIncreaseStat();
        }
        return result;
    }
}