using System;
using System.Collections.Generic;

public interface IEquipmentable
{
    public event Action<EquipmentBase.PARTS> OnSetEquipment;

    Dictionary<EquipmentBase.PARTS, EquipmentBase> MyEquipment { get; }
}
