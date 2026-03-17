
using UnityEngine;


public abstract class ItemBase
{
    public abstract void Init(int id);
}


public interface IEnhancement
{
    public ItemBaseSO ItemData { get; }
    public int EnhanceLevel { get; }
    public ItemEnhanceData EnhanceData { get; }

    public void OnEnhance(EnhancingInfo result);
}