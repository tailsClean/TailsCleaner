
using UnityEngine;

public abstract class PassiveModifier
{
    // 기본 스탯에 추가
    // ex) 목표를 중앙에 두고 스위치
    public virtual void ModifyBaseAdd(SkillStat baseStat) { }

    // 패시브 스탯 배율 합
    // 패시브 배율 합을 업그레이드 스탯 계산 후 곱함
    // ex) 더 크게, 스노우 볼링, 임플란트
    public virtual void ModifyMul(SkillStat mulSumStat) { }

    // 최종 곱산용
    // 최종 스탯에 최종 곱산
    // ex) 양손잡이, 냥빨래, 황금왕관
    public virtual void ModifyFinal(SkillStat finalStat) { }


    public virtual bool OnProjectileInit(SkillStat runtimeBaseStat, SkillStat runtimeFinalStat) { return false; }   // 투사체 생성 시, bool은 재계산 확인용
    public virtual void OnPierce(SkillStat passiveMulStat) { }  // 관통 시
    public virtual void OnTick(SkillStat passiveMulStat) { }    // 틱 데미지 시
    public virtual void OnExpire(SkillStat finalStat) { }       // 투사체 만료 시
}




// ID 42002 / SubTag 40102
// 목표를 중앙에 두고 스위치 (투사체 속도 증가, 넉백 강화)
public class CenterSwitchModifier : PassiveModifier
{
    [Header("추가 속도")] public float ProjectileSpeedBonus = 1f;
    [Header("필요 넉백")] public float RequireKnockback = 1f;
    [Header("추가 넉백")] public float KnockbackBonus = 2f;

    public override void ModifyBaseAdd(SkillStat baseStat)
    {
        baseStat.ProjectileSpeed += ProjectileSpeedBonus;

        // 넉백 조건부 추가는 투사체 Init 에서 초기 finalStat 보고 처리
    }

    public override bool OnProjectileInit(SkillStat runtimeBaseStat, SkillStat runtimeFinalStat)
    {
        // 투사체 생성 시 넉백이 조건 이상이면
        if (runtimeFinalStat.Knockback >= RequireKnockback)
        {
            // 추가 넉백을 기본 스탯에 추가
            runtimeBaseStat.Knockback += KnockbackBonus;
            return true;
        }
        return false;
    }
}


// ID 42004 / SubTag 40104
// 추가 추가 피해 (추가 피해 * 2)
public class DoubleExtraDamageModifier : PassiveModifier
{
    [Header("추가 횟수")] public int ExtraDamageMultiplier = 1;
    public override void ModifyBaseAdd(SkillStat baseStat)
    {
        // 1회 추가
        baseStat.ExtraDamageMultiplier = ExtraDamageMultiplier;
    }
}

// ID 42014 / SubTag 40114
// 기초적인 임플란트입니다 (투사체 관통 시 추가 피해 부여)
public class ImplantModifier : PassiveModifier
{
    [Header("관통 추가 피해 계수")] public float DamagePerPierce = 0.2f;

    public override void OnPierce(SkillStat passiveMulAcc)
    {
        passiveMulAcc.Damage += DamagePerPierce; // 1.0 -> 1.2 -> 1.4
    }
}


// ID 42016 / SubTag 40116
// 냥빨래 (넉백 강화, 밀친 적 화면 밖으로 나갈 시 체력 비례 고정 피해)

public class CatLaundryModifier : PassiveModifier
{
    [Header("추가 넉백 배율")] public float KnockbackBonus = 2f;
    [Header("체력 비례 피해")] public float OffScreenDamageRatio = 0.1f;

    public override void ModifyFinal(SkillStat finalStat)
    {
        finalStat.Knockback *= KnockbackBonus;
    }
}

