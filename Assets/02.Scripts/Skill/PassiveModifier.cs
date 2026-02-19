
public abstract class PassiveModifier
{
    public int PassiveId;               // passive_skill_id
    public int SubTag;                  // 서브 태그
    //public PassiveConfigBase Config;    // 패시브 수치

    public void Init(int id)
    {
        PassiveId = id;
        //Config = SkillDataLoader.GetPassiveConfig((PASSIVE_ID)PassiveId);
    }

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

    // 패시브 ID
    public enum PASSIVE_ID
    {
        RaccoonCrate = 42001,           // 매이크 라쿤 크레이트 어겐!
        CenterSwitch = 42002,           // 목표를 중앙에 두고 스위치
        FocusAttack = 42003,            // 집중공략
        DoubleExtraDmg = 42004,         // 추가 추가 피해
        SuperClean = 42005,             // SuperClean
        Bravado = 42006,                // 객기
        VinylCoat = 42007,              // 청소용 비닐옷
        ClassicSecret = 42008,          // 고전비급
        BiggerBetter = 42009,           // 더 크게! 더! 더더! 크고 아름답게!
        GoldenCrown = 42010,            // 크고 아름다운 황금 왕관!(물에 뜹니다.)
        ADCarry = 42011,                // 원딜의 정석
        Snowballing = 42012,            // 스노우볼링
        Ambi = 42013,                   // 양손잡이
        Implant = 42014,                // 기초적인 임플란트입니다
        SodaWater = 42015,              // 탄산수
        CatLaundry = 42016,             // 냥빨래
        NimbleBlock = 42017,            // 하지만 이렇게 간단하게 피했습니다.
    }
}




// ID 42002 / SubTag 40102
// 목표를 중앙에 두고 스위치 (투사체 속도 증가, 넉백 강화)
public class CenterSwitchModifier : PassiveModifier
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

public class CatLaundryModifier : PassiveModifier
{
    public override void ModifyStatAdd(ActiveSkill skill, SkillStat baseStat)
    {
        // 일단 수치 대충
        baseStat.Knockback += 1f;
    }
}

