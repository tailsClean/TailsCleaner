using System;
using System.Collections.Generic;

public interface IEquipmentable
{
    public event Action<EquipmentBase.PARTS> OnSetEquipment;
    public event Action<RelicBase> OnSetRelic;

    Dictionary<EquipmentBase.PARTS, EquipmentBase> MyEquipment { get; }
    public List<RelicBase> MyRelic { get; }
}
