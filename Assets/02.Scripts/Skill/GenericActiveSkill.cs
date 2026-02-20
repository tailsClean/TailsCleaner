using UnityEngine;

public abstract class GenericActiveSkill<TController, TData> : ActiveSkill
    where TController : Component
    where TData : class, new()
{
    protected TController _skillPrefabComponent;
    public TData ModifierData = new TData();
    
    // 프리팹 캐싱
    public override void Init(ActiveSkillData skillData, ActiveUpgradeData upgradeData, GameObject prefab)
    {
        // 부모 Init 먼저
        base.Init(skillData, upgradeData, prefab);

        if (_skillPrefab != null)
        {
            _skillPrefabComponent = _skillPrefab.GetComponent<TController>();
        }
    }

    // 데이터 갱신
    public override void ApplyUpgrade(ActiveUpgradeData upgradeData)
    { 
        base.ApplyUpgrade(upgradeData);

        // 모디파이어 데이터 갱신
        ModifierData = new TData();
        foreach (var mod in _modifiers)
        {
            // 모디파이어 적용
            mod.Apply(this);
        }

        // 패시브 재적용 (CalculateStats + ApplyPassiveLogics)
        // 스탯 재계산, 로직 재적용
        RecheckPassives();

        Debug.Log($"[ActiveSkill] 업그레이드 완료: [{SkillDataLoader.GetActiveSkillData(MainTag).SkillName}] (MainTag : {MainTag})\n" +
                  $" - 업그레이드 : {upgradeData.Name} (Active_Skill_ID : {upgradeData.Id})\n" +
                  $" - 업그레이드 Lv : {GetUpgradeLevel(upgradeData.Id)} / {upgradeData.MaxLevel}\n" +
                  $" - 스킬 전체 Lv : {CurrentLevel} / {MAX_SKILL_LEVEL}");
    }
}
