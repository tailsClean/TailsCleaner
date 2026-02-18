public abstract class PassiveModifier
{
    public int PassiveId;   // passive_skill_id
    public int SubTag;      // 서브 태그

    // 스탯 계산 곱
    // 황금왕관(* 3), 양손잡이(* 0.5) 등
    public virtual void ModifyStatMul(ActiveSkill skill, SkillStat baseStat) { }

    // 스탯 계산 합
    // 목표를 중앙에 두고 스위치(+ 1) 냥빨래(+ 1) 등
    public virtual void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat) { }

    // 로직 적용, 업그레이드 시 호출
    // 스노우볼링, 냥빨래 등
    public void ModifyLogic(ActiveSkill skill)
    {
        skill.ActivePassiveIds.Add(PassiveId);
    }



    // 패시브 ID 상수
    public const int PASSIVE_CENTER_SWITCH   = 42002;
    public const int PASSIVE_EXTRA_DAMAGE    = 42004;
    public const int PASSIVE_IMPLANT         = 42014;
    public const int PASSIVE_LAUNDRY         = 42016;
}




// ID 42002 / SubTag 40102
// 목표를 중앙에 두고 스위치 (투사체 속도 증가, 넉백 강화)
public class TargetCenterSwitchModifier : PassiveModifier
{
    public override void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat)
    {
        // 일단 수치 대충
        baseStat.ProjectileSpeed += 1f;

        if (baseStat.Knockback > 0)
            baseStat.Knockback += 2;
    }
}


// ID 42004 / SubTag 40104
// 추가 추가 피해 (추가 피해 * 2)
public class DoubleExtraDamageModifier : PassiveModifier { }

// ID 42014 / SubTag 40114
// 기초적인 임플란트입니다 (투사체 관통 시 추가 피해 부여)
public class ImplantModifier : PassiveModifier { }


// ID 42016 / SubTag 40116
// 냥빨래 (넉백 강화, 밀친 적 화면 밖으로 나갈 시 체력 비례 고정 피해)

public class LaundryModifier : PassiveModifier
{
    public override void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat)
    {
        // 일단 수치 대충
        baseStat.Knockback += 1f;
    }
}

