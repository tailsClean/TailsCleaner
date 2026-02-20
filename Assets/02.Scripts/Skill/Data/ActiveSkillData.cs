using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveData", menuName = "Skill/ActiveData")]
public class ActiveSkillData : ScriptableObject
{

    [Header("기본 정보")]
    public int MainTag;
    public string SkillName;
    public ATTACK_TYPE AttackType;
    public TARGETING_TYPE TargetingType;

    [Header("스킬 프리팹")]
    public ActiveSkill SkillPrefab;

    [Header("투사체 프리팹")]
    public GameObject SkillProjectilePrefab;

    [Header("업그레이드별 모디파이어 설정")]
    public List<UpgradeModifierData> UpgradeModifierDatas = new();


    public enum ATTACK_TYPE  // 공격 방식
    {
        None = 0,
        Area = 4101,        // 장판형
        Projectile = 4102,  // 투사체형
    }

    public enum TARGETING_TYPE // 조준 방식
    {
        None = 0,
        NonTarget = 4201,   // 비대상형   (공격 방향)
        Closest = 4202,     // 조준형     (공격 방향 가장 가까운 적)
        Barrier = 4203,     // 배리어형   (플레이어 기준)
        Directional = 4204, // 이동방향형 (이동 방향)
    }
}



[Serializable]
public class UpgradeModifierData
{
    public string Name;             // active_upgrade_name    (자동)

    public int UpgradeId;           // active_skill_id        (자동)

    [TextArea(1, 3)]
    public string Desc;             // effect                 (자동)

    // 전용 모디파이어
    [SerializeReference] public ActiveModifier Modifier;

    // 특수 로직 수치 설정
    [SerializeReference] public ActiveModifierConfig Config;
}