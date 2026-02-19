using System;
using UnityEngine;

[Serializable]
public class PassiveModifierConfig { }

[Serializable]  // 42002 목표를 중앙에 두고 스위치
public class CenterSwitchConfig : PassiveModifierConfig
{
    [Tooltip("추가 속도")] public float ProjectileSpeedBonus = 1f;
    [Tooltip("필요 넉백")] public float RequireKnockback = 1f;
    [Tooltip("추가 넉백")] public float KnockbackBonus = 2f;
}

// 42004 추가 추가 피해
[Serializable] public class DoubleExtraDamageConfig : PassiveModifierConfig { }


[Serializable]  // 42014 기초적인 임플란트입니다
public class ImplantConfig : PassiveModifierConfig
{
    public float DamagePerPierce = 0.2f;
}


[Serializable]  // 42016 냥빨래
public class CatLaundryConfig : PassiveModifierConfig
{
    public float KnockbackBonus = 1f;
    public float OffScreenDamageRatio = 0.1f;
}