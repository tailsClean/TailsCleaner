
// 40011 감나빗!    /      재추적
using UnityEngine;

public class SoapRetargetModifier : ActiveModifier<SoapThrowSkill>
{
    public override void ApplyModifier(SoapThrowSkill skill)
    {
        // 재추적 설정
        skill.ModifierData.Retracking = true;
    }
}

// 40012 거품내기    /      관통 시 추가 피해
public class SoapPierceDamageModifier : ActiveModifier<SoapThrowSkill>
{
    [Header("관통당 추가 피해")]
    public float DamagePerPierce = 0.5f;

    public override void ApplyModifier(SoapThrowSkill skill)
    {
        // 관통 추가 피해 설정
        skill.ModifierData.PierceDamage = true;
        skill.ModifierData.DamagePerPierce += DamagePerPierce;
    }
}

// 40014 거품 가속   /      관통 시 속도 증가
public class SoapPierceSpeedModifier : ActiveModifier<SoapThrowSkill>
{
    [Header("관통당 추가 속도")]
    public float SpeedPerPierce = 1f;

    public override void ApplyModifier(SoapThrowSkill skill)
    {
        // 관통 추가 속도 설정
        skill.ModifierData.PierceSpeed = true;
        skill.ModifierData.SpeedPerPierce += SpeedPerPierce;
    }
}

// 40015 비누 덩어리  /     관통 제거
public class SoapRemovalPierceModifier : ActiveModifier<SoapThrowSkill>
{
    [Header("감소 관통당 추가 피해")]
    public float DamagePerRemovalPierce = 2f;

    public override void ApplyModifier(SoapThrowSkill skill)
    {
        // 관통 제거 설정
        skill.ModifierData.RemovePierce = true;
        skill.ModifierData.DamagePerRemovalPierce += DamagePerRemovalPierce;
    }
}