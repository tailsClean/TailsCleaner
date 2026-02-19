using System;
using System.Collections.Generic;
using UnityEngine;
using static ActiveBaseData;

[CreateAssetMenu(fileName = "ActiveData", menuName = "Skill/ActiveData")]
public class ActiveSkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int MainTag;
    public string SkillName;
    public ATTACK_TYPE AttackType;
    public TARGETING_TYPE TargetingType;

    [Header("투사체 프리팹 (수동)")]
    public GameObject SkillProjectilePrefab;

    [Header("업그레이드별 모디파이어 설정")]
    public List<UpgradeModifierConfig> UpgradeConfigs = new();
}



[Serializable]
public class UpgradeModifierConfig
{
    public string Name;             // active_upgrade_name    (자동)

    public int UpgradeId;           // active_skill_id        (자동)

    [TextArea(1, 3)]
    public string Desc;             // effect                 (자동)

    // 특수 로직 수치 설정
    [SerializeReference] public ActiveModifierConfig Config;
}