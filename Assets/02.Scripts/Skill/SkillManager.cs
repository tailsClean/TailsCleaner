using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using static ActiveSkillBaseData;

[RequireComponent(typeof(UpgradeSelect))]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public const int MAX_ACTIVE_SLOTS = 6;      // 최대 액티브 스킬 수
    public const int MAX_PASSIVE_SLOTS = 6;     // 최대 패시브 스킬 수

    // 모든 액티브 스킬 업그레이드 데이터
    public List<ActiveSkillUpgradeData> AllActiveUpgradeData { get; private set; } = new List<ActiveSkillUpgradeData>();

    // 액티브 스킬 베이스 데이터 (공격 방식, 조준 방식)
    // Key : MainTag
    private Dictionary<int, ActiveSkillBaseData> _activeBaseDatas = new Dictionary<int, ActiveSkillBaseData>();

    // 플레이어 보유 스킬 리스트
    public List<ActiveSkill> MyActiveSkills { get; private set; } = new List<ActiveSkill>();
    public List<PassiveSkill> MyPassiveSkills { get; private set; } = new List<PassiveSkill>();

    // 액티브 슬롯 체크
    public bool IsActiveSlotFull => MyActiveSkills.Count >= MAX_ACTIVE_SLOTS;

    // 태그별 스킬 스탯 합
    // Key : MainTag, SubTag
    private Dictionary<int, SkillStatBonus> _skillBonuses = new Dictionary<int, SkillStatBonus>();

    private void Awake() { Instance = this; }

    private void Start()
    {
        // 서브 태그 설정
        InitSubTagRegistry();

        // 테스트용 더미 데이터 (나중에 CSV로드로 교체) 
        LoadDummyData();

        // 기본 스킬 추가. 기획서 상에서는 못봤는데 기본 공격이 없으면 공격 못하니까 일단 추가
        AddDefaultSkill();

    }


    // 보유중인 메인 태그의 액티브 스킬 반환
    public ActiveSkill GetActiveSkill(int mainTag)
    {
        // 보유 중인 액티브 스킬 중
        for (int i = 0; i < MyActiveSkills.Count; i++)
        {
            // 입력 태그와 같은 스킬 있으면 반환
            if (MyActiveSkills[i].MainTag == mainTag) return MyActiveSkills[i];
        }

        // 없으면 null 반환
        return null;
    }

    // 메인 태그와 티어로 업그레이드 데이터 반환
    public ActiveSkillUpgradeData FindUpgradeData(int mainTag, int tier)
    {
        for (int i = 0; i < AllActiveUpgradeData.Count; i++)
        {
            if (AllActiveUpgradeData[i].MainTag == mainTag && AllActiveUpgradeData[i].Tier == tier)
                return AllActiveUpgradeData[i];
        }
        return null;
    }

    // 메인 태그의 액티브 스킬 기본 데이터 반환
    public ActiveSkillBaseData GetBaseData(int mainTag)
    {
        if (_activeBaseDatas.TryGetValue(mainTag, out var data))
            return data;

        Debug.LogError($"[Manager] 메인 태그 {mainTag}에 해당하는 BaseData가 없음.");
        return null;
    }

    // 모든 스킬 만렙인지 확인
    public bool IsAllActiveMaxLevel()
    {
        // 액티브 스킬 없으면
        if (MyActiveSkills.Count == 0) return false;

        // 활성화된 액티브 스킬 순회
        for (int i = 0; i < MyActiveSkills.Count; i++)
        {
            // 액티브 스킬이 최대 레벨 보다 낮으면
            if (MyActiveSkills[i].CurrentLevel < ActiveSkill.MAX_SKILL_LEVEL) return false;
        }

        // 전부 다 만렙이라면
        return true;
    }

    // 선택지 적용
    public void ApplyOption(ActiveSkillUpgradeData upgradeData)
    {
        // 0티어 신규 생성
        if (upgradeData.Tier == 0)
        {
            // 오브젝트 생성 후 이름 변경
            GameObject go = new GameObject($"Skill_{upgradeData.MainTag}");
            // 일단 스킬 매니저 하위로
            go.transform.SetParent(transform);

            // 액티브 붙이기
            ActiveSkill newSkill = go.AddComponent<ActiveSkill>();

            // 액티브 기본 데이터
            ActiveSkillBaseData baseData = GetBaseData(upgradeData.MainTag);

            // 스킬 초기화
            newSkill.Init(baseData, upgradeData);

            // 보유 스킬에 추가
            if (MyActiveSkills.Contains(newSkill) == false)
            {
                MyActiveSkills.Add(newSkill);
            }
        }
        // 1티어 이상 스킬 업그레이드
        else
        {
            // 보유 중인 액티브 스킬인지 체크
            ActiveSkill skill = GetActiveSkill(upgradeData.MainTag);

            // 유효하면 업그레이드 데이터 적용
            if (skill != null)
            {
                skill.ApplyUpgrade(upgradeData);
            }
        }
    }

    #region 초기화

    // 테스트용 임시 더미 데이터 생성
    private void LoadDummyData()
    {
        // 기본 데이터 생성
        AddBaseData(41001, "비누 거품", ATTACK_TYPE.Area, TARGETING_TYPE.Barrier);
        AddBaseData(41002, "비누 던지기", ATTACK_TYPE.Projectile, TARGETING_TYPE.Closest);

        // 업그레이드 데이터 생성

        // 비누 거품
        AddData(40001, "비누 거품", 41001, 0, 1, 0, 0, 0.3f);
        AddData(40002, "자동 추척 비누 지우개", 41001, 1, 10, 40102);
        AddData(40003, "버블버블", 41001, 2, 1, 40101);
        AddData(40004, "빨래당함", 41001, 2, 1, 40103);
        AddData(40005, "흐르는 거품", 41001, 3, 1, 40112);
        // 비누 던지기
        AddData(41009, "비누 던지기", 41002, 0, 1, 0, 0, 1f);
        AddData(41010, "미끄러지기", 41002, 1, 10, 40114);
        AddData(41011, "감나빗!", 41002, 2, 10, 40114, 40102);

        Debug.Log($"[SkillManager] 로드 완료. ({AllActiveUpgradeData.Count}개)");
    }

    // 대충 기본 데이터 생성
    private void AddBaseData(int mainTag, string name, ATTACK_TYPE aType, TARGETING_TYPE tType)
    {
        _activeBaseDatas.Add(mainTag, new ActiveSkillBaseData(mainTag, name, aType, tType));
    }

    // 대충 업그레이드 데이터 생성
    private void AddData(int id, string name, int mainTag, int tier, int maxLevel, int subTag1, int subTag2 = 0, float dmg = 0)
    {
        var data = new ActiveSkillUpgradeData
        {
            Id = id,
            Name = name,
            MainTag = mainTag,
            Tier = tier,
            MaxLevel = maxLevel,
            SubTag1 = subTag1,
            SubTag2 = subTag2,
        };

        AllActiveUpgradeData.Add(data);
    }

    // 서브 태그 초기화 (임시)
    private void InitSubTagRegistry()
    {
        SubTagRegistry.Init(new List<int> { 40101, 40102, 40103, 40112, 40114});
    }


    // 시작 기본 스킬
    private void AddDefaultSkill()
    {
        ActiveSkillUpgradeData defaultSkill = AllActiveUpgradeData.Find(x => x.Id == 41009);
        if (defaultSkill != null)
        {
            Debug.Log("기본 스킬 지급");
            ApplyOption(defaultSkill);
        }
        else
        {
            Debug.LogError("기본 스킬 데이터 찾을 수 없음.");
        }
    }

    #endregion
}
