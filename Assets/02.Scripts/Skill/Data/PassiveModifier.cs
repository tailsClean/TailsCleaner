
using UnityEngine;

public abstract class PassiveModifier
{
    // 스탯 계산 곱
    // 황금왕관(* 3), 양손잡이(* 0.5) 등
    public virtual void ModifyStatMul(ActiveSkill skill, SkillStat baseStat) { }

    // 스탯 계산 합
    // 목표를 중앙에 두고 스위치(+ 1) 냥빨래(+ 1) 등
    public virtual void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat) { }
}




// ID 42002 / SubTag 40102
// 목표를 중앙에 두고 스위치 (투사체 속도 증가, 넉백 강화)
public class CenterSwitchModifier : PassiveModifier
{
    [Header("추가 속도")] public float ProjectileSpeedBonus = 1f;
    [Header("필요 넉백")] public float RequireKnockback = 1f;
    [Header("추가 넉백")] public float KnockbackBonus = 2f;

    public override void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat)
    {
        // 일단 수치 대충
        baseStat.ProjectileSpeed += ProjectileSpeedBonus;

        if (baseStat.Knockback >= RequireKnockback)
            baseStat.Knockback += KnockbackBonus;
    }
}


// ID 42004 / SubTag 40104
// 추가 추가 피해 (추가 피해 * 2)
public class DoubleExtraDamageModifier : PassiveModifier
{
    [Header("추가 횟수")] public int ExtraDamageTimes = 1;
}

// ID 42014 / SubTag 40114
// 기초적인 임플란트입니다 (투사체 관통 시 추가 피해 부여)
public class ImplantModifier : PassiveModifier
{
    [Header("추가 피해")] public float DamagePerPierce = 0.2f;
}


// ID 42016 / SubTag 40116
// 냥빨래 (넉백 강화, 밀친 적 화면 밖으로 나갈 시 체력 비례 고정 피해)

public class CatLaundryModifier : PassiveModifier
{
    [Header("추가 넉백")] public float KnockbackBonus = 1f;
    [Header("체력 비례 피해")] public float OffScreenDamageRatio = 0.1f;

    public override void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat)
    {
        // 일단 수치 대충
        baseStat.Knockback += KnockbackBonus;
    }
}

