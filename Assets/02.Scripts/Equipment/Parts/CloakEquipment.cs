using UnityEngine;

public class CloakEquipment : Equipment
{
    [SerializeField] private int _maxHpIncrease;
    [SerializeField] private int _defensePowerIncrease;

    public int MaxHpIncrease => _maxHpIncrease;
    public int DefensePowerIncrease => _defensePowerIncrease;
}