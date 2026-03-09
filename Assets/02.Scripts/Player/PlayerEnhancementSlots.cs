using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerEnhancementSlots
{
    private Dictionary<EQUIP_PARTS, EquipmentBase> _myEquipments;
    private List<RelicBase> _myRelics;

    private Dictionary<RELIC_STAT, int> _relicncreaseValue;

    public PlayerEnhancementSlots(
        Dictionary<EQUIP_PARTS, EquipmentBase> equipments, 
        List<RelicBase> relics
        )
    {
        _myEquipments = equipments;
        _myRelics = relics;
        _relicncreaseValue = new();

        if (_myRelics != null)
            SetRelicncreaseValue();

        if (equipments == null)
            Debug.LogWarning("<color=red>플레이어 장비가 null입니다.</color>");
        if( relics == null )
            Debug.Log("<color=yellow>장착한 유물이 없습니다.</color>");
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
    public int GetIncreaseStat(RELIC_STAT stat) => _relicncreaseValue[stat];

    // 클래스 생성시, 유물의 스텟 증가량을 미리 합산
    private void SetRelicncreaseValue()
    {
        _relicncreaseValue.Add(RELIC_STAT.GoldGainRate, 0);
        _relicncreaseValue.Add(RELIC_STAT.ItemDropRate, 0);
        _relicncreaseValue.Add(RELIC_STAT.ExpGainRate, 0);

        foreach(var relic in _myRelics)
        {
            RELIC_STAT stat = relic.Data.StatType;
            _relicncreaseValue[stat] += relic.GetIncreaseStat(stat);
        }
    }
}