using System;
using System.Collections.Generic;

public interface IEquipmentable
{
    public event Action<Equipment.PARTS> OnSetEquipment;

    Dictionary<Equipment.PARTS, Equipment> MyEquipment { get; }
}
