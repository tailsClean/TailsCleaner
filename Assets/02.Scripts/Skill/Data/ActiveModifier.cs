using System;

[Serializable]
public abstract class ActiveModifier
{
    // 특수 로직 적용
    public abstract void Apply(ActiveSkill skill);
}




[Serializable]
public abstract class ActiveModifier<T> : ActiveModifier where T : ActiveSkill
{
    // T 형변환
    public override void Apply(ActiveSkill skill)
    {
        if (skill is T specificSkill)
        {
            ApplyModifier(specificSkill);
        }
    }

    // 실제 특수 로직 적용
    public abstract void ApplyModifier(T skill);
}