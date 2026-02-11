using UnityEngine;


// 40011 감나빗! / 재추적
public class SoapRetargetModifier : SkillModifier
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


 // 40015 비누 덩어리 / 관통 제거
 // public class SoapRemovePierceModifier : SkillModifier