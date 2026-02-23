using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment
{
    private Dictionary<Equipment.PARTS, Equipment> _myEquipments;

    public PlayerEquipment(Dictionary<Equipment.PARTS, Equipment> equipments)
    {
        _myEquipments = equipments;
        if (equipments == null)
            Debug.LogWarning("플레이어 장비가 null입니다.");
    }

    public int GetMoveSpeedIncrease()
    {
        if (_myEquipments == null || _myEquipments[Equipment.PARTS.Shoes] == null)
            return 0;

        var shoes = _myEquipments[Equipment.PARTS.Shoes].ApplyEquipment<ShoesEquipment>();
        if( shoes == null )
            return 0;

        return shoes.MoveSpeedIncrease;
    }
}