using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentTest : MonoBehaviour, IEquipmentable
{
    public Dictionary<PlayerBase.EQUIPMENT, PlayerEquipment> MyEquipment { get; private set; } = new();

    public event Action<PlayerBase.EQUIPMENT> OnSetEquipment;

    // 장비를 교체하는 메서드
    public void SetEquipment(PlayerEquipment equipment)
    {
        if (!MyEquipment.TryAdd(equipment.EquipmentPart, equipment))
            MyEquipment[equipment.EquipmentPart] = equipment;

        OnSetEquipment?.Invoke(equipment.EquipmentPart);
    }
}
