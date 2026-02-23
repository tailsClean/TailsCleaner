using UnityEngine;

public class WeaponEquipment : Equipment
{
    [SerializeField] private int _attackPowerIncrease;

    public int AttackPowerIncrease => _attackPowerIncrease;
}