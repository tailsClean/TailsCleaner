using UnityEngine;
using System.Collections.Generic;

public class PlayerEquipment
{
    private Dictionary<EquipmentBase.PARTS, EquipmentBase> _myEquipments;

    public PlayerEquipment(Dictionary<EquipmentBase.PARTS, EquipmentBase> equipments)
    {
        _myEquipments = equipments;
        if (equipments == null)
            Debug.LogWarning("플레이어 장비가 null입니다.");
    }

    public int GetMoveSpeedIncrease()
    {
        // if (_myEquipments.TryGetValue(EquipmentBase.PARTS.Shoes, out var shoes))
        //     return shoes.GetIncreaseStat(EquipmentIncreaseStat.STAT.MoveSpeed);


            return 0;
    }
}