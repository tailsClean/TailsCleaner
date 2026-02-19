using System;
using System.Collections.Generic;

public interface IEquipmentable
{
    public event Action<PlayerBase.EQUIPMENT> OnSetEquipment;

    Dictionary<PlayerBase.EQUIPMENT, PlayerEquipment> MyEquipment { get; }
}
