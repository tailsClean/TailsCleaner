using UnityEngine;

public abstract class GenericActiveSkill<TController, TData> : ActiveSkill
    where TController : Component
    where TData : class, new()
{
    protected TController _skillPrefabComponent;
    public TData ModifierData = new TData();
    
    // 프리팹 캐싱
    protected virtual void Start()
    {
        if (_skillPrefab != null) _skillPrefabComponent = _skillPrefab.GetComponent<TController>();
    }

    // 데이터 갱신
    public override void ApplyUpgrade(ActiveUpgradeData data)
    { 
        base.ApplyUpgrade(data);

        // 모디파이어 데이터 갱신
        ModifierData = new TData();
        foreach (var mod in _modifiers)
        {
            // 모디파이어 적용
            mod.Apply(this);
        }
    }
}
