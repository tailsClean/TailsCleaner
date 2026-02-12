using System;
using System.Collections.Generic;

internal interface IEquipmentable
{
    public event Action<PlayerBase.EQUIPMENT> OnSetEquipment;

    Dictionary<PlayerBase.EQUIPMENT, PlayerEquipment> MyEquipment { get; }

    void SetEquipment(PlayerEquipment equipment);
}
