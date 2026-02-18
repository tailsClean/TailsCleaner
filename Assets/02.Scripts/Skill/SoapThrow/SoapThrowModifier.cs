using UnityEngine;


// 40011 감나빗!    /      재추적
public class SoapRetargetModifier : ActiveModifier
{
    public override void Apply(ActiveSkill skill)
    {
        // 비누 던지기인지 확인
        if (skill is SoapThrowSkill soapSkill)
        {
            // 재추적 설정
            soapSkill.ModifierData.Retracking = true;
        }
    }
}

// 40012 거품내기    /      관통 시 추가 피해
public class SoapPierceDamageModifier : ActiveModifier
{
    public override void Apply(ActiveSkill skill)
    {
        if (skill is SoapThrowSkill soapSkill)
        {
            // 관통 추가 피해 설정
            soapSkill.ModifierData.PierceDamage = true;
        }
    }
}

// 40014 거품 가속   /      관통 시 속도 증가
public class SoapPierceSpeedModifier : ActiveModifier
{
    public override void Apply(ActiveSkill skill)
    {
        if (skill is SoapThrowSkill soapSkill)
        {
            // 관통 추가 속도 설정
            soapSkill.ModifierData.PierceSpeed = true;
        }
    }
}

// 40015 비누 덩어리  /     관통 제거
public class SoapRemovePierceModifier : ActiveModifier
{
    public override void Apply(ActiveSkill skill)
    {
        if (skill is SoapThrowSkill soapSkill)
        {
            // 관통 제거 설정
            soapSkill.ModifierData.RemovePierce = true;
        }
    }
}