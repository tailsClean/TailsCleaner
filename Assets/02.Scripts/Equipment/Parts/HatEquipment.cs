using UnityEngine;

public class HatEquipment : Equipment
{
    [SerializeField] private int _criticalChanceIncrease;

    public int CriticalChanceIncrease => _criticalChanceIncrease;
}