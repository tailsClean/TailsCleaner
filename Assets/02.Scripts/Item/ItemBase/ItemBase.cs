using Unity.Android.Gradle.Manifest;
using UnityEngine;


public abstract class ItemBase
{
    public abstract void Init(int id);
}


public interface IEnhancement
{
    public int EnhanceLevel { get; }
    public EnhanceData EnhanceData { get; }
}