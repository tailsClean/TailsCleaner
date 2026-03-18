
using UnityEngine;


public abstract class ItemBase
{
    public abstract void Init(int id);
}


public interface IEnhancement
{
    public int EnhanceLevel { get; }
    //public EquipEnhance EnhanceData { get; }

    public void OnEnhance(EnhancingInfo result);
}