using System;
using UnityEngine;

[Serializable]
public class ActiveModifierConfig { }



#region 비누 던지기 (MainTag 41002)
[Serializable]
public class SoapPierceDamageConfig : ActiveModifierConfig      // 거품내기 (40012)
{
    [Tooltip("관통당 추가 데미지")] public float DamagePerPierce = 0.2f;
}
[Serializable]
public class SoapPierceSpeedConfig : ActiveModifierConfig       // 거품 가속 (40014)
{
    [Tooltip("관통당 추가 속도")] public float SpeedPerPierce = 1f;
}
#endregion

