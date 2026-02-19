using System.Collections.Generic;
using UnityEngine;

public static class SkillDataLoader     // 사실상 스킬 데이터 매니저
{
    public static Dictionary<int, ActiveSkillData> ActiveSkillMap { get; private set; } = new();
    public static Dictionary<int, PassiveSkillData> PassiveSkillMap { get; private set; } = new();

    private static Dictionary<int, ActiveModifierConfig> _upgradeConfigMap = new();
    private static Dictionary<int, PassiveModifierConfig> _passiveConfigMap = new();

    private const string ACTIVE_PATH = "Active";
    private const string PASSIVE_PATH = "Passive";

    // 스킬 전체 불러오기
    public static void Init()
    {
        LoadActiveSkills();
        LoadPassiveSkills();
        Debug.Log($"[SkillDataLoader] 액티브 {ActiveSkillMap.Count}개, 패시브 {PassiveSkillMap.Count}개 로드 완료.");
    }
    
    // 액티브 스킬 불러오기
    private static void LoadActiveSkills()
    {
        ActiveSkillMap.Clear();
        _upgradeConfigMap.Clear();
    
        // 액티브 스킬 경로에 모든 SO 불러오기
        foreach (var so in Resources.LoadAll<ActiveSkillData>(ACTIVE_PATH))
        {
            // 메인태그에 SO 등록
            if (ActiveSkillMap.TryAdd(so.MainTag, so) == false)
            {
                // 혹시나 중복이면 스킵
                Debug.LogWarning($"[SkillDataLoader] 중복 MainTag: {so.MainTag} ({so.name})");
                continue;
            }

            // 스킬의 업그레이드 설정 순회
            foreach (var upgradeConfig in so.UpgradeConfigs)
            {
                // 업그레이드 ID 에 설정 추가
                if (upgradeConfig.Config != null)
                    _upgradeConfigMap.TryAdd(upgradeConfig.UpgradeId, upgradeConfig.Config);
            }
        }
    }

    // 패시브 스킬 불러오기
    private static void LoadPassiveSkills()
    {
        PassiveSkillMap.Clear();
        _passiveConfigMap.Clear();

        // 패시브 스킬 경로에 모든 SO 불러오기
        foreach (var so in Resources.LoadAll<PassiveSkillData>(PASSIVE_PATH))
        {
            // 패시브 ID 에 SO 등록
            if (PassiveSkillMap.TryAdd(so.PassiveId, so) == false)
            {
                // 혹시나 중복이면 스킵
                Debug.LogWarning($"[SkillDataLoader] 중복 PassiveId: {so.PassiveId} ({so.name})");
                continue;
            }

            // 패시브 ID 에 설정 추가
            if (so.Config != null) _passiveConfigMap.TryAdd(so.PassiveId, so.Config);
        }
    }
    

    // 스킬 데이터
    public static ActiveSkillData GetActiveSkillData(int mainTag)
        => ActiveSkillMap.TryGetValue(mainTag, out var data) ? data : null;
    public static PassiveSkillData GetPassiveSkillData(int passiveId)
        => PassiveSkillMap.TryGetValue(passiveId, out var data) ? data : null;


    // 설정 데이터
    public static ActiveModifierConfig GetActiveConfig(int upgradeId)
        => _upgradeConfigMap.TryGetValue(upgradeId, out var config) ? config : null;
    
    public static PassiveModifierConfig GetPassiveConfig(int passiveId)
        => _passiveConfigMap.TryGetValue(passiveId, out var config) ? config : null;
    
    // 스킬
    //public static GameObject GetSkillPrefab(int mainTag)
    //    => ActiveSkillMap.TryGetValue(mainTag, out var so) ? so.SkillPrefab : null;
    
    // 투사체
    public static GameObject GetSkillPrefab(int mainTag)
        => ActiveSkillMap.TryGetValue(mainTag, out var so) ? so.SkillProjectilePrefab : null;
}