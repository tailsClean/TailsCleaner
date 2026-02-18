using System.Collections.Generic;
using System;
using UnityEngine;

public static class PassiveModifierRegistry
{
    // 패시브 모디파이어 목록
    private static Dictionary<int, Type> _passiveModifier = new();

    // 초기화
    public static void Init()
    {
        // 42002 목표를 중앙에 두고 스위치 (추적 40102)
        Register(42002, typeof(TargetCenterSwitchModifier));

        // 42004 추가 추가 피해 (추가피해 40104)
        Register(42004, typeof(DoubleExtraDamageModifier));
        
        // 42014 기초적인 임플란트입니다 (관통 강화 40114)
        Register(42014, typeof(ImplantModifier));
        
        // 42016 냥빨래 (넉백 강화 40116)
        Register(42016, typeof(LaundryModifier));

        Debug.Log($"[PassiveModifierRegistry] 패시브 모디파이어 {_passiveModifier.Count}개 등록 완료.");
    }

    // 패시브 모디파이어 등록
    public static void Register(int passiveId, Type modifierType)
    {
        if (_passiveModifier.ContainsKey(passiveId))
        {
            Debug.LogWarning($"[PassiveModifierRegistry] ID {passiveId} 중복 등록 ({modifierType.Name})");
            return;
        }
        _passiveModifier.Add(passiveId, modifierType);
    }

    // 패시브 획득 시 호출
    // 없으면 null (순수 스탯/플레이어 효과)
    public static PassiveModifier Create(int passiveId, int subTag)
    {
        // 등록된 모디파이어 있는지 확인
        if (_passiveModifier.TryGetValue(passiveId, out Type type))
        {
            // 있으면 생성
            PassiveModifier modifier = Activator.CreateInstance(type) as PassiveModifier;

            // ID, SubTag 주입
            if (modifier != null)
            {
                modifier.PassiveId = passiveId;
                modifier.SubTag = subTag;
                return modifier;
            }
        }

        return null;
    }
}
