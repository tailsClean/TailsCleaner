using System.Collections.Generic;
using UnityEngine;
using static ActiveBaseData;

public abstract class ActiveSkill : MonoBehaviour
{
    public const int MAX_SKILL_LEVEL = 10;                  // 스킬 최대 레벨

    public int MainTag { get; private set; }                // 메인 태그
    public int CurrentLevel { get; private set; } = 0;      // 현재 레벨
    public int CurrentSubTag { get; private set; } = 0;     // 적용 중인 서브 태그

    [Header("스킬 프리팹")]
    [SerializeField] protected GameObject _skillPrefab;

    // 스킬 타입
    protected ATTACK_TYPE _attackType;
    protected TARGETING_TYPE _targetingType;

    
    protected SkillStat _baseStat = new();                           // 기본 스탯
    
    protected SkillStat _commonStat = SkillStat.CreateMultiplier();  // 공용 스탯 (곱연산)
    
    protected SkillStat _upgradeStat = new();                        // 업그레이드 스탯 (합연산)
    
    protected SkillStat _finalStat = new();                          // 최종 스탯  (_baseStat * _commonStat) + _upgradeStat

    // 모디파이어 목록
    protected List<SkillModifier> _modifiers = new();

    // 업그레이드 별 레벨 (Key: active_skill_id, Value: UpgradeLevel)
    private Dictionary<int, int> _upgradeLevels = new();

    protected float _lastActiveTime = 0f; // 최근 스킬 실행 시간

    // 초기화 (0티어 획득)
    public void Init(ActiveBaseData baseData, ActiveUpgradeData upgradeData, GameObject prefab)
    {   
        // 메인 태그
        MainTag = baseData.MainTag;

        // 타입 설정
        _attackType = baseData.AttackType;
        _targetingType = baseData.TargetingType;
        Debug.Log($"[ActiveSkill] {baseData.Name} 타입 설정 완료. ({_attackType}, {_targetingType})");

        // 스킬의 사용 프리팹
        _skillPrefab = prefab;
        Debug.Log($"[ActiveSkill] {baseData.Name} 의 프리팹 {_skillPrefab.name} 설정 완료.");

        // 레벨 1 시작
        CurrentLevel = 1;

        // 기본 스탯
        _baseStat = upgradeData.GetSkillStat();

        Debug.Log($"[ActiveSkill] 업그레이드 기본 스탯 Speed : {_baseStat.ProjectileSpeed}");

        // 스탯 계산
        CalculateStats();

        Debug.Log($"[ActiveSkill] {upgradeData.Name} 생성 완료.");
    }

    protected virtual void Update()
    {
        // 발동 가능 체크
        if (CanActive() == true)
        {
            // 발동
            Active();
            _lastActiveTime = Time.time;
        }
    }
    
    // 발동 가능 체크
    protected virtual bool CanActive()
    {
        // 혹시 몰라서 일단 최소 쿨타임 잡아 놓음
        float cooldown = Mathf.Max(0.1f, _finalStat.Cooldown);
        return Time.time >= _lastActiveTime + cooldown;
    }

    // 스킬 발동 로직 (자식에서)
    protected abstract void Active();

    // 스킬 업그레이드
    public virtual void ApplyUpgrade(ActiveUpgradeData upgradeData)
    {
        // 업그레이드의 서브 태그 추가
        if (upgradeData.SubTag1 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag1);
        if (upgradeData.SubTag2 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag2);

        // 스킬 레벨, 업그레이드 레벨 증가
        LevelUp(upgradeData);

        // 스탯 누적
        AddStat(upgradeData);

        // 모디파이어 추가
        AddModifier(upgradeData.Id);

        // 스탯 갱신
        CalculateStats();

        Debug.Log($"[ActiveSkill] 업그레이드 완료: [{SkillManager.Instance.ActiveBaseDatas[MainTag].Name}] (MainTag : {MainTag})\n" +
                  $" - 업그레이드 : {upgradeData.Name} (Active_Skill_ID : {upgradeData.Id})\n" +
                  $" - 업그레이드 Lv : {GetUpgradeLevel(upgradeData.Id)} / {upgradeData.MaxLevel}\n" +
                  $" - 스킬 전체 Lv : {CurrentLevel} / {MAX_SKILL_LEVEL}");
    }

    // 레벨 증가
    private void LevelUp(ActiveUpgradeData upgradeData)
    {
        // 레벨 증가
        CurrentLevel++;

        // 업그레이드 처음일 시 추가
        if (_upgradeLevels.ContainsKey(upgradeData.Id) == false)
        {
            _upgradeLevels.Add(upgradeData.Id, 0);
        }

        // 업그레이드 레벨 증가
        _upgradeLevels[upgradeData.Id]++;
    }


    // 스탯 누적
    private void AddStat(ActiveUpgradeData upgradeData)
    {
        // 공용
        if (upgradeData.MainTag == SkillManager.COMMON_MAIN_TAG)
        {
            // 곱연산 누적
            SkillStat multiplier = upgradeData.GetSkillStat();
            _commonStat.Multiply(multiplier);
        }
        // 전용
        else
        {
            // 합연산 누적
            _upgradeStat.Add(upgradeData.GetSkillStat());
        }
    }

    // 모디파이어 추가
    private void AddModifier(int upgradeId)
    {
        // 모디파이어 생성 후 추가
        SkillModifier modifier = SkillModifierRegistry.Create(upgradeId);

        if (modifier != null)
        {
            _modifiers.Add(modifier);
        }
    }

    // 최종 스탯 계산
    protected void CalculateStats()
    {
        // 초기화
        _finalStat = new SkillStat();

        // 계산 (_baseStat * _commonStat) + _upgradeStat
        _finalStat.Add(_baseStat);
        _finalStat.Multiply(_commonStat);
        _finalStat.Add(_upgradeStat);

        Debug.Log($"최종 공격력 : {_finalStat.Damage} = ( {_baseStat.Damage} * {_commonStat.Damage} ) + {_upgradeStat.Damage}");

        // 추가로 플레이어, 패시브 스탯 계산하면 될 듯
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
