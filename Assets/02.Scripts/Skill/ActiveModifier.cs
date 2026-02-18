public abstract class ActiveModifier
{
    // active_skill_id
    public int UpgradeId;

    // 특수 로직 적용
    public abstract void Apply(ActiveSkill skill);
}
