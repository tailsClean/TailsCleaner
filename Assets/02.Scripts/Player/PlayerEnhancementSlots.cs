using System.Collections.Generic;
using UnityEngine;
using static EquipmentBase;


/// <summary>
/// 플레이어의 장비, 유물 인벤토리
/// </summary>
public class PlayerEnhancementSlots
{
    private Dictionary<EquipmentBase.PARTS, EquipmentBase> _myEquipments;
    private List<RelicBase> _myRelics;

    private Dictionary<RelicBase.STAT, int> _relicncreaseValue;

    public PlayerEnhancementSlots(
        Dictionary<EquipmentBase.PARTS, EquipmentBase> equipments, 
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
    public int GetIncreaseStat(EquipmentBase.STAT stat)
    {
        if (_myEquipments == null)
            return 0;

        EquipmentBase equipment = null;
        switch (stat)
        {

            case EquipmentBase.STAT.AttackPower:
                equipment = _myEquipments[PARTS.Weapon];
                break;

            case EquipmentBase.STAT.CriticalChance:
                equipment = _myEquipments[PARTS.Hat];
                break;

            case EquipmentBase.STAT.MaxHp or EquipmentBase.STAT.DefensePower:
                equipment = _myEquipments[PARTS.Cloak];
                break;

            case EquipmentBase.STAT.MoveSpeed or EquipmentBase.STAT.EvasionChance:
                equipment = _myEquipments[PARTS.Shoes];
                break;

            default:
                return 0;
        }

        return equipment.GetIncreaseStat(stat);
    }

    // 유물리스트의 스텟 증가량을 반환
    public int GetIncreaseStat(RelicBase.STAT stat) => _relicncreaseValue[stat];

    // 클래스 생성시, 유물의 스텟 증가량을 미리 합산
    private void SetRelicncreaseValue()
    {
        _relicncreaseValue.Add(RelicBase.STAT.GoldGainRate, 0);
        _relicncreaseValue.Add(RelicBase.STAT.ItemDropRate, 0);
        _relicncreaseValue.Add(RelicBase.STAT.ExpGainRate, 0);

        foreach(var relic in _myRelics)
        {
            RelicBase.STAT stat = relic.StatUp;
            _relicncreaseValue[stat] += relic.GetIncreaseStat();
        }
    }
}