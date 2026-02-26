
using UnityEngine;

public class  EquipmentIncreaseStat
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public STAT Type { get; private set; }
    public int Value { get; private set; }

    public EquipmentIncreaseStat(EquipmentBase equip)
    {
        ID = equip.StatID;
        GroupID = equip.StatGroupID;
        Type = equip.StatType;
        Value = equip.StatValue;
    }

    public enum STAT
    {
        AttackPower, CriticalChance, MaxHp, DefensePower, MoveSpeed, EvasionChance
    }
}