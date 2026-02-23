using System.Collections.Generic;
using UnityEngine;
using static ActiveSkillData;

public abstract class ActiveSkill : MonoBehaviour
{
    public const int MAX_SKILL_LEVEL = 10;                  // 스킬 최대 레벨
    public const float MIN_SKILL_COOLDOWN = 0.1f;           // 스킬 최소 쿨타임

    public int MainTag { get; private set; }                // 메인 태그
    public int CurrentLevel { get; private set; } = 0;      // 현재 레벨
    public int CurrentSubTag { get; private set; } = 0;     // 적용 중인 서브 태그

    protected GameObject _skillPrefab;  // 스킬 장판,투사체

    // 스킬 타입
    protected ATTACK_TYPE _attackType;
    protected TARGETING_TYPE _targetingType;

    // 외부 참조용 (패시브 모디파이어, 스킬 투사체)
    public SkillStat BaseStat => _baseStat;
    public SkillStat CommonStat => _commonStat;
    public SkillStat UpgradeStat => _upgradeStat;
    public SkillStat PassiveMulStat => _passiveMulStat;
    public SkillStat FinalStat => _finalStat;


    protected SkillStat _baseStat = new();                              // 기본 스탯
    protected SkillStat _commonStat = SkillStat.CreateMultiplier();     // 공용 스탯    
    protected SkillStat _upgradeStat = new();                           // 업그레이드 스탯
    protected SkillStat _passiveMulStat = SkillStat.CreateMultiplier(); // 패시브 배율 합
    protected SkillStat _finalStat = new();                             // 최종 스탯
    // ((baseStat + 깡 추가 패시브 스탯) * 공용 스탯 + (업그레이드 스탯 * 추가추가피해 패시브)) * 패시브 스탯 배율 합 * 최종 배율 (황금왕관, 양손잡이, 냥빨래)


    // 전용 모디파이어 목록
    protected List<ActiveModifier> _skillModifiers = new();
    // 패시브 모디파이어 목록
    public List<PassiveModifier> PassiveModifiers { get; private set; } = new();



    // 업그레이드 별 레벨 (Key: active_skill_id, Value: UpgradeLevel)
    protected Dictionary<int, int> _upgradeLevels = new();

    protected float _lastActiveTime = 0f; // 최근 스킬 실행 시간

    // 초기화 (0티어 획득)
    public virtual void Init(ActiveSkillData skillData, ActiveUpgradeData upgradeData, GameObject prefab)
    {   
        // 메인 태그
        MainTag = skillData.MainTag;

        // 서브 태그
        AddSubTag(upgradeData);

        // 타입 설정
        _attackType = skillData.AttackType;
        _targetingType = skillData.TargetingType;
        Debug.Log($"[ActiveSkill] {skillData.SkillName} 타입 설정 완료. ({_attackType}, {_targetingType})");

        // 스킬의 사용 프리팹
        _skillPrefab = prefab;
        Debug.Log($"[ActiveSkill] {skillData.SkillName} 의 프리팹 {_skillPrefab.name} 설정 완료.");

        // 레벨 1 시작
        CurrentLevel = 1;

        // 기본 스탯
        _baseStat = upgradeData.GetSkillStat();

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
        // 최소 쿨타임 0.1초
        float cooldown = Mathf.Max(MIN_SKILL_COOLDOWN, _finalStat.Cooldown);
        return Time.time >= _lastActiveTime + cooldown;
    }

    // 스킬 발동 로직 (자식에서)
    protected abstract void Active();

    // 스킬 업그레이드
    public virtual void ApplyUpgrade(ActiveUpgradeData upgradeData)
    {
        AddSubTag(upgradeData);         // 업그레이드의 서브 태그 추가
        LevelUp(upgradeData);           // 스킬 레벨, 업그레이드 레벨 증가
        AddStat(upgradeData);           // 공용, 업그레이드 스탯 누적
        AddModifier(upgradeData.Id);    // 전용 모디파이어 추가

        // 자식인 GenericActiveSkill에서
        // 전용 모디파이어 다 설정하고
        // 패시브 설정하면서 스탯, 로직 적용(RecheckPassives)
    }


    // 서브 태그 추가

    private void AddSubTag(ActiveUpgradeData upgradeData)
    {
        // 업그레이드의 서브 태그 추가
        if (upgradeData.SubTag1 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag1);
        if (upgradeData.SubTag2 != 0) CurrentSubTag |= SubTagRegistry.GetFlag(upgradeData.SubTag2);
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
        if (upgradeData.MainTag == SkillDataLoader.COMMON_MAIN_TAG)
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

    // 전용 모디파이어 추가
    private void AddModifier(int upgradeId)
    {
        // 전용 모디파이어 생성 후 추가
        ActiveModifier modifier = SkillDataLoader.GetActiveModifier(upgradeId);
        
        if (modifier != null)
        {
            _skillModifiers.Add(modifier);
        }
    }

    // 초기화, 업그레이드 시 스탯 설정
    protected void CalculateStats()
    {
        _finalStat = GetFinalStat(_baseStat, _commonStat, _upgradeStat, _passiveMulStat);
    }

    // 최종 스탯 계산 후 반환 (초기화, 업그레이드, 투사체 내부 로직)
    public SkillStat GetFinalStat(SkillStat baseStat, SkillStat commonStat, SkillStat upgradeStat, SkillStat passiveMulStat)
    {
        // 기본 스탯 생성
        SkillStat resultStat = baseStat.Clone();

        // 패시브 스탯 (합)
        foreach (var passive in SkillManager.Instance.MyPassiveSkills)
        {
            // 서브 태그 매치 안되면 스킵
            if (IsPassiveMatch(passive) == false) continue;

            // 아니면 기본 스탯에 합
            passive.Modifier.ModifyBaseAdd(resultStat);
        }

        // 공용 스탯 (곱)
        resultStat.Multiply(commonStat);

        // 업그레이드 스탯 (합)
        resultStat.Add(upgradeStat);

        // 패시브 배율 합 (곱)
        resultStat.Multiply(passiveMulStat);

        // 최종 배율 (곱)
        foreach (var passive in SkillManager.Instance.MyPassiveSkills)
        {
            // 서브 태그 매치 안되면 스킵
            if (IsPassiveMatch(passive) == false) continue;

            // 아니면 스탯에 곱
            passive.Modifier.ModifyFinal(resultStat);
        }

        // 최종 스탯 = ((baseStat + 패시브 깡 스탯) * 공용 스탯 + (업그레이드 스탯 * 추가추가피해 패시브)) * 패시브 스탯 배율 합 * 최종 배율 (황금왕관, 양손잡이, 냥빨래)
        Debug.Log($"최종 공격력 : {resultStat.Damage} = (( {baseStat.Damage}(기본) + 패시브 깡 스탯) * {commonStat.Damage}(공용)  + ({upgradeStat.Damage}(업그레이드) * 추가추가피해)) * {passiveMulStat.Damage}(패시브 배율) * 패시브 최종 배율");

        // 최종 결과 스탯 반환
        return resultStat;
    }

    // 패시브 스탯 배율 합 설정
    private void SetPassiveMulStat()
    {
        // 기본 생성
        _passiveMulStat = SkillStat.CreateMultiplier();

        // 패시브 순회하면서
        foreach (var passive in SkillManager.Instance.MyPassiveSkills)
        {
            // 배율 합치기
            if (IsPassiveMatch(passive) == false) continue;
            passive.Modifier.ModifyMul(_passiveMulStat);
        }
    }

    // 패시브 적용
    protected void AddPassiveModifier()
    {
        PassiveModifiers.Clear();

        // 보유 패시브 순회
        foreach (var passive in SkillManager.Instance.MyPassiveSkills)
        {
            if (IsPassiveMatch(passive) == false) continue;

            PassiveModifiers.Add(passive.Modifier);
        }
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

    // 패시브 재적용
    // 스킬 업그레이드, 패시브 습득 시 호출
    public void RecheckPassives()
    {
        SetPassiveMulStat();    // 패시브 배율 합
        CalculateStats();       // finalStat 계산
        AddPassiveModifier();   // 패시브 모디파이어
    }
    
    // 패시브 서브태그가 현재 스킬에 해당하는지
    private bool IsPassiveMatch(PassiveSkillData passive)
    {
        int flag = SubTagRegistry.GetFlag(passive.SubTag);
        return flag != 0 && (CurrentSubTag & flag) != 0;
    }
}

// 프리팹과 모디파이어 타입 결정
public abstract class ActiveSkill<TSkillObject, TModifierData> : ActiveSkill
    where TSkillObject : Component
    where TModifierData : class, new()
{
    protected TSkillObject _skillObjectPrefab;
    public TModifierData _modifierData = new TModifierData();

    // 프리팹 캐싱
    public override void Init(ActiveSkillData skillData, ActiveUpgradeData upgradeData, GameObject prefab)
    {
        // 부모 Init 먼저
        base.Init(skillData, upgradeData, prefab);

        if (_skillPrefab != null)
        {
            _skillObjectPrefab = _skillPrefab.GetComponent<TSkillObject>();
        }
    }

    // 데이터 갱신
    public override void ApplyUpgrade(ActiveUpgradeData upgradeData)
    {
        base.ApplyUpgrade(upgradeData);

        // 모디파이어 데이터 갱신
        _modifierData = new TModifierData();
        foreach (var mod in _skillModifiers)
        {
            // 모디파이어 적용
            mod.Apply(this);
        }

        // 패시브 재적용 (CalculateStats + ApplyPassiveModifier)
        // 스탯 재계산, 로직 재적용
        RecheckPassives();

        Debug.Log($"[ActiveSkill] 업그레이드 완료: [{SkillDataLoader.GetActiveSkillData(MainTag).SkillName}] (MainTag : {MainTag})\n" +
                  $" - 업그레이드 : {upgradeData.Name} (Active_Skill_ID : {upgradeData.Id})\n" +
                  $" - 업그레이드 Lv : {GetUpgradeLevel(upgradeData.Id)} / {upgradeData.MaxLevel}\n" +
                  $" - 스킬 전체 Lv : {CurrentLevel} / {MAX_SKILL_LEVEL}");
    }
}
