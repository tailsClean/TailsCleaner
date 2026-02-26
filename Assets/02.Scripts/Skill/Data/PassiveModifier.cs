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
    public virtual void OnDamage(MonsterBase monster) { }                             // 적 피해 시
    public virtual void OnPierce(SkillStat runTimePassiveMulStat) { }                 // 관통 시
    public virtual void OnEnterArea(MonsterBase monster) { }                          // 장판 올라갈 시 (몬스터)
    public virtual void OnEnterArea(PlayerBase player) { }                            // 장판 올라갈 시 (플레이어)
    public virtual void OnDurationTick(SkillStat runTimePassiveMulStat) { }           // 지속시간마다
    public virtual void OnStun(MonsterBase monster) { }                               // 군중제어
}


// ID 42001 / SubTag 40101
// 매이크 라쿤 크레이트 어겐! (보유 업그레이드 40101 태그 수만큼 방어력, 회피율, 치명타율, 치명타 피해 증가)
public class RaccoonCrateModifier : PassiveModifier
{
    [Header("강화 태그 1개당 증가량")]
    public int DefencePerTag = 2;
    public float EvasionChancePerTag = 0.01f;       // 1%
    public float CriticalChancePerTag = 0.01f;      // 1%
    public float CriticalDamagePerTag = 0.05f;    // 5%

    private int _lastTagCount;  // 최근 태그 수

    //Defence         = DefencePerTag* tagCount,
    //EvasionChance = EvasionChancePerTag * tagCount,
    //CriticalChance = CriticalChancePerTag * tagCount,
    //CriticalDamage = CriticalDamagePerTag * tagCount,
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

// ID 42003 / SubTag 40103
// 집중공략 (약화(슬로우 등)된 적은 최대 체력 5% 감소)
public class FocusAttackModifier : PassiveModifier
{
    [Header("최대 체력 감소율")]
    public float MaxHpDecreaseRate = 0.05f;
    
    public override void OnEnterArea(MonsterBase monster)
    {
        //monster.DecreaseMaxHp(MaxHpDecreaseRate);
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
        baseStat.ExtraDamageMultiplier += ExtraDamageMultiplier;
    }
}

// ID 42005 / SubTag 40105
// SuperClean (군중제어 스킬이 적 속도를 5초간 추가로 느려지게 함)
public class SuperCleanModifier : PassiveModifier
{
    [Header("추가 이동속도 감소율")]
    public float SlowAmount = 0.2f;
    [Header("추가 슬로우 지속시간")]
    public float SlowDuration = 5f;

    public override void OnStun(MonsterBase monster)
    {
        // 군중제어 서브태그의 효과 발동 시
        // 적 속도 추가로 느려지게 적용
        //monster.ApplySlow(SlowAmount, SlowDuration);
    }
}

// ID 42009 / SubTag 40109
// 더 크게! 더! 더더! 크고 아름답게! (투사체 크기에 비례해 넉백과 데미지 계수가 증가)
public class BiggerBetterModifier : PassiveModifier
{

}


// ID 42010 / SubTag 40110
// 크고 아름다운 황금 왕관!(물에 뜹니다.)  (데미지 계수가 3배, 플레이어의 이동 속도 절반)
public class GoldCrownModifier : PassiveModifier
{
    [Header("데미지 계수")]
    public float DamageMultiplier = 3f;
    [Header("이동속도 계수")]
    public float SpeedMultiplier = 0.5f;

    public override void ModifyFinal(SkillStat finalStat)
    {
        finalStat.Damage *= DamageMultiplier;
        finalStat.ProjectileSpeed *= SpeedMultiplier;
    }
}

// ID 42011 / SubTag 40111
// 원딜의 정석 (공격 시전 속도에 비례해 치명타 확률 증가)
public class ADCarryModifier : PassiveModifier
{

}


// ID 42012 / SubTag 40112
// 스노우볼링 (스킬 지속 시간 동안 일정 시간마다 스탯 증가)
public class SnowballingModifier : PassiveModifier
{
    [Header("배율 증가 간격")]
    public float TickInterval = 0.5f;
    [Header("틱당 배율 증가량")]
    public float MultiplierPerTick = 0.2f;
    public override void ModifyBaseAdd(SkillStat baseStat)
    {
        baseStat.DurationTickInterval = TickInterval;
    }

    public override void OnDurationTick(SkillStat runTimePassiveMulStat)
    {
        runTimePassiveMulStat.Damage            += MultiplierPerTick;
        runTimePassiveMulStat.Size              += MultiplierPerTick;
        runTimePassiveMulStat.ProjectileSpeed   += MultiplierPerTick;
    }
}


// ID 42013 / SubTag 40113
// 양손잡이 (데미지 계수 절반. 투사체가 두배)
public class AmbiModifier : PassiveModifier
{
    [Header("데미지 계수")]
    public float DamageMultiplier = 0.5f;
    [Header("투사체 계수")]
    public int ProjectileCountMultiplier = 2;

    public override void ModifyFinal(SkillStat finalStat)
    {
        finalStat.Damage *= DamageMultiplier;
        finalStat.ProjectileCount *= ProjectileCountMultiplier;
    }
}



// ID 42014 / SubTag 40114
// 기초적인 임플란트입니다 (투사체 관통 시 추가 피해 부여)
public class ImplantModifier : PassiveModifier
{
    [Header("관통 추가 피해 계수")] public float DamagePerPierce = 0.2f;

    public override void OnPierce(SkillStat runTimePassiveMulSum)
    {
        runTimePassiveMulSum.Damage += DamagePerPierce; // 1.0 -> 1.2 -> 1.4
    }
}

// ID 42015 / SubTag 40115
// 탄산수 (데미지 틱이 피해를 입힐 때 마다 적의 최대 체력비례 피해)
public class SodaWaterModifier : PassiveModifier
{

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

