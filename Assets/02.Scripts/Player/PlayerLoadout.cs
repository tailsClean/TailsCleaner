using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerLoadout
{
    private Dictionary<EQUIP_PARTS, EquipmentBase> _myEquipments;
    private List<RelicBase> _myRelics;

    public Dictionary<EQUIP_PARTS, EquipmentBase> MyEquipments => _myEquipments;
    public List<RelicBase> MyRelics => _myRelics;

    public PlayerLoadout()
    {
        _myRelics = new List<RelicBase>(3);
        _myEquipments = new Dictionary<EQUIP_PARTS, EquipmentBase>
        {
            {EQUIP_PARTS.Weapon, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultWeapon)},
            {EQUIP_PARTS.Hat, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultHat)},
            {EQUIP_PARTS.Cloak, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultCloak)},
            {EQUIP_PARTS.Shoes, ItemDB.CreateItem<EquipmentBase>(ItemID.DefaultShose)}
        };
    }

    // 장비의 스텟 증가량을 반환
    public float GetIncreaseStat(EQUIP_STAT stat)
    {
        if (_myEquipments == null)
            return 0;

        EquipmentBase equipment = null;
        switch (stat)
        {
            case EQUIP_STAT.AttackPower:
                equipment = _myEquipments[EQUIP_PARTS.Weapon];
                break;

            case EQUIP_STAT.CriticalChance:
                equipment = _myEquipments[EQUIP_PARTS.Hat];
                break;

            case EQUIP_STAT.MaxHp or EQUIP_STAT.DefensePower:
                equipment = _myEquipments[EQUIP_PARTS.Cloak];
                break;

            case EQUIP_STAT.MoveSpeed or EQUIP_STAT.EvasionChance:
                equipment = _myEquipments[EQUIP_PARTS.Shoes];
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