using System;
using System.Collections.Generic;
using UnityEngine;

public static class ActiveModifierRegistry
{
    // 업그레이드 모디파이어
    // Key : active_skill_id, Value : 모디파이어 컴포넌트
    private static Dictionary<int, Type> _upgradeModifier = new();

    // 초기화
    public static void Init()
    {
        // -----비누 거품-----
        // 자동 추적 비누 지우개 (적 추적)
        //Register(40002, );

        // 버블버블 (플레이어 방어력 버프)
        //Register(40003, );

        // 빨래당함 (적 슬로우)
        //Register(40004, );



        // -----비누 던지기-----
        // 감나빗! (관통 후 재추적)
        Register(40011, typeof(SoapRetargetModifier));
        // 거품내기 (관통 시 추가 피해)
        Register(40012, typeof(SoapPierceDamageModifier));
        // 거품 가속 (관통 시 추가 속도)
        Register(40014, typeof(SoapPierceSpeedModifier));
        // 비누 덩어리 (관통 제거, 감소한 만큼 추가 피해)
        Register(40016, typeof(SoapRemovePierceModifier));

        Debug.Log($"[ModifierRegistry] 특수 로직 모디파이어 {_upgradeModifier.Count}개 등록 완료.");
    }


    // 모디파이어 등록
    public static void Register(int activeSkillId, Type modifierType)
    {
        if (_upgradeModifier.ContainsKey(activeSkillId))
        {
            Debug.LogWarning($"[ModifierRegistry] ID {activeSkillId}가 중복 등록됨. ({modifierType.Name})");
            return;
        }

        _upgradeModifier.Add(activeSkillId, modifierType);
    }

    // 생성 (액티브 생성 시 호출)
    public static ActiveModifier Create(int activeSkillId)
    {
        // 등록된 모디파이어 있는지 확인
        if (_upgradeModifier.TryGetValue(activeSkillId, out Type type))
        {
            // 있으면 생성 (리플렉션)
            ActiveModifier modifier = Activator.CreateInstance(type) as ActiveModifier;
            if (modifier != null) return modifier;
        }

        // 없으면 null 반환 (단순 스탯 업그레이드)
        return null;
    }
}
