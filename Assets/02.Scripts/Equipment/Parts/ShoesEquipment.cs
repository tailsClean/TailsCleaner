using UnityEngine;

public class ShoesEquipment : Equipment
{
    [SerializeField] private int _moveSpeedIncrease;
    [SerializeField] private int _evasionChanceIncrease;

    public int MoveSpeedIncrease => _moveSpeedIncrease;
    public int EvasionChanceIncrease => _evasionChanceIncrease;
}