using UnityEngine;


// 비누 던지기 모디파이어
public class SoapModifierData
{
    public bool Retracking = false;
}


public class SoapThrowSkill : ActiveSkill
{
    public SoapModifierData ModifierData;

    // 업그레이드 시 모디파이어 설정
    public override void ApplyUpgrade(ActiveUpgradeData upgradeData)
    {
        base.ApplyUpgrade(upgradeData);

        // 초기화
        ModifierData = new SoapModifierData();

        // 모디파이어 순회해서 적용
        foreach (var modifier in _modifiers)
        {
            modifier.Apply(this);
        }
    }
}
