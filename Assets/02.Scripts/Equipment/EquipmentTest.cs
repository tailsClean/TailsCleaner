using System;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentTest : MonoBehaviour, IEquipmentable
{
    [SerializeField] private EquipmentEventChannelSO _onWearEquipment;

    public Dictionary<EquipmentBase.PARTS, EquipmentBase> MyEquipment { get; private set; } = new();

    public event Action<EquipmentBase.PARTS> OnSetEquipment;

    private void OnEnable()
    {
        _onWearEquipment.AddListener(SetEquipment);
    }

    private void OnDisable()
    {
        _onWearEquipment.RemoveListener(SetEquipment);
    }

    //
    private void Update()
    {
        PlayerDataTransfer.SetEquipments(MyEquipment);
    }
    //

    // 장비를 교체하는 메서드
    public void SetEquipment(EquipmentBase equipment)
    {
        if (!MyEquipment.TryAdd(equipment.EquipmentPart, equipment))
            MyEquipment[equipment.EquipmentPart] = equipment;

        OnSetEquipment?.Invoke(equipment.EquipmentPart);
    }
}
