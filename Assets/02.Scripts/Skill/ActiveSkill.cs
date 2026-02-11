using System.Collections.Generic;
using UnityEngine;
using static ActiveSkillBaseData;

public class ActiveSkill : MonoBehaviour
{
    public const int MAX_SKILL_LEVEL = 10;                  // 스킬 최대 레벨

    public int MainTag { get; private set; }                // 메인 태그
    public int CurrentLevel { get; private set; } = 0;      // 현재 레벨
    public int CurrentSubTag { get; private set; } = 0;     // 적용 중인 서브 태그

    // 스킬 타입
    private ATTACK_TYPE _attackType;
    private TARGETING_TYPE _targetingType;

    // 스킬 스탯 보너스
    private SkillStatBonus _statBonus = new SkillStatBonus();
    
    // 업그레이드 별 레벨 (Key: active_skill_id, Value: UpgradeLevel)
    private Dictionary<int, int> _upgradeLevels = new Dictionary<int, int>();

    // 초기화
    public void Init(ActiveSkillBaseData baseData, ActiveSkillUpgradeData upgradeData)
    {   
        // 메인 태그
        MainTag = baseData.MainTag;

        // 타입 설정
        _attackType = baseData.AttackType;
        _targetingType = baseData.TargetingType;
        Debug.Log($"[ActiveSkill] {baseData.Name} 타입 설정완료 ({_attackType}, {_targetingType})");

        // 0 티어도 업그레이드 취급
        ApplyUpgrade(upgradeData);      
        Debug.Log($"[ActiveSkill] {upgradeData.Name} 생성 완료.");
    }

    // 스킬 업그레이드
    public void ApplyUpgrade(ActiveSkillUpgradeData upgradeData)
    {
        // 업그레이드의 서브 태그 추가
        if (upgradeData.SubTag1 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag1);
        if (upgradeData.SubTag2 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag2);

        // 레벨 증가
        CurrentLevel++;

        // 업그레이드 처음일 시 추가
        if (_upgradeLevels.ContainsKey(upgradeData.Id) == false)
        {   
            _upgradeLevels.Add(upgradeData.Id, 0);
        }

        // 업그레이드 레벨 증가
        _upgradeLevels[upgradeData.Id]++;

        // 스탯 누적
        _statBonus.Add(upgradeData.GetSkillBonus());

        Debug.Log($"[ActiveSkill] {MainTag} 번 액티브 스킬 업그레이드 완료.");
    }

    // 업그레이드의 현재 레벨
    public int GetUpgradeLevel(int upgradeId)
    {
        // 업그레이드 기록 있으면
        if (_upgradeLevels.TryGetValue(upgradeId, out int level))
        {
            // 현재 레벨 반환
            return level;
        }

        // 없으면 0
        return 0;
    }
}
