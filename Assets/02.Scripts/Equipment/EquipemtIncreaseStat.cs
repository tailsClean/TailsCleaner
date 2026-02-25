
using UnityEngine;

public class EquipmentIncreaseStat
{
    public int ID { get; private set; }
    public int GroupID { get; private set; }
    public STAT Type { get; private set; }
    public int Value { get; private set; }

    public EquipmentIncreaseStat(int id, int groupID, STAT type, int value)
    {
        ID = id;
        GroupID = groupID;
        Type = type;
        Value = value;
    }

    public enum STAT
    {
        AttackPower, CriticalChance, MaxHp, DefensePower, MoveSpeed, EvasionChance
    }
}