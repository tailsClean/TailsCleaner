using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentTest : MonoBehaviour, IEquipmentable
{
    [SerializeField] private EquipmentEventChannelSO _onChangeEquipment;

    public Dictionary<PlayerBase.EQUIPMENT, PlayerEquipment> MyEquipment { get; private set; } = new();

    public event Action<PlayerBase.EQUIPMENT> OnSetEquipment;

    private void OnEnable()
    {
        _onChangeEquipment.AddListener(SetEquipment);
    }

    private void OnDisable()
    {
        _onChangeEquipment.RemoveListener(SetEquipment);
    }

    // 장비를 교체하는 메서드
    public void SetEquipment(PlayerEquipment equipment)
    {
        if (!MyEquipment.TryAdd(equipment.EquipmentPart, equipment))
            MyEquipment[equipment.EquipmentPart] = equipment;

        OnSetEquipment?.Invoke(equipment.EquipmentPart);
    }
}
