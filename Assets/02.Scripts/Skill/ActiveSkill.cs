using System.Collections;
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

    protected string _poolTag;          // 풀 태그 (자식 스킬에서 투사체 생성 시 사용)

    protected GameObject _skillPrefab;  // 스킬 장판,투사체

    // 스킬 타입
    protected ATTACK_TYPE _attackType;
    protected TARGETING_TYPE _targetingType;

    // 외부 참조용 (패시브 모디파이어, 스킬 투사체, 타겟)
    public SkillStat BaseStat => _baseStat;
    public SkillStat CommonStat => _commonStat;
    public SkillStat UpgradeStat => _upgradeStat;
    public SkillStat PassiveMulStat => _passiveMulStat;
    public SkillStat FinalStat => _finalStat;
    public Transform CurrentTarget => _currentTarget;


    protected SkillStat _baseStat = new();                              // 기본 스탯
    protected SkillStat _commonStat = SkillStat.CreateMultiplier();     // 공용 스탯    
    protected SkillStat _upgradeStat = new();                           // 업그레이드 스탯
    protected SkillStat _passiveMulStat = SkillStat.CreateMultiplier(); // 패시브 배율 합
    protected SkillStat _finalStat = new();                             // 최종 스탯
    // ((baseStat + 깡 추가 패시브 스탯) * 공용 스탯 + (업그레이드 스탯 * 추가추가피해 패시브)) * 패시브 스탯 배율 합 * 최종 배율 (황금왕관, 양손잡이, 냥빨래)


    // 전용 모디파이어 목록
    protected List<(ActiveModifier modifier, ActiveUpgradeData upgradeData)> _skillModifiers = new();
    // 패시브 모디파이어 목록
    public List<PassiveModifier> PassiveModifiers { get; private set; } = new();



    // 업그레이드 별 레벨 (Key: active_skill_id, Value: UpgradeLevel)
    protected Dictionary<int, int> _upgradeLevels = new();

    protected float _lastActiveTime = 0f; // 최근 스킬 실행 시간
    protected WaitForSeconds _fireDelay;    // 순차 발사 딜레이

    protected Transform _currentTarget = null;      // 타겟
    protected Coroutine _searchCoroutine = null;    // 탐색 코루틴


    [Header("스킬 실행 간격")]
    [SerializeField] protected float _fireInterval = 0.1f;      // 여러 투사체 발사 시 텀
    [Header("조준형 설정")]
    [SerializeField] protected float _angle = 15f;              // 범위 각도
    [SerializeField] protected float _distance = 50f;           // 타겟 탐색 거리




    private void Awake()
    {
        _fireDelay = new WaitForSeconds(_fireInterval);
    }


    // 초기화 (0티어 획득)
    public virtual void Init(ActiveSkillData skillData, ActiveUpgradeData upgradeData, GameObject prefab)
    {   
        // 메인 태그
        MainTag = skillData.MainTag;

        // 풀 태그 string 캐싱
        _poolTag = MainTag.ToString();

        // 서브 태그
        AddSubTag(upgradeData);

        // 타입 설정
        _attackType = skillData.AttackType;
        _targetingType = skillData.TargetingType;
        //Debug.Log($"[ActiveSkill] {skillData.SkillName} 타입 설정 완료. ({_attackType}, {_targetingType})");

        // 스킬의 사용 프리팹
        _skillPrefab = prefab;
        //Debug.Log($"[ActiveSkill] {skillData.SkillName} 의 프리팹 {_skillPrefab.name} 설정 완료.");

        // 레벨 1 시작
        CurrentLevel = 1;

        // 기본 스탯
        _baseStat = upgradeData.GetSkillStat();

        Debug.Log($"[ActiveSkill] {upgradeData.Name} 생성 완료.");
    }

    protected virtual void Update()
    {
        // 조준형 타겟팅
        Targeting();

        // 쿨타임이 됐고 발동 조건도 맞으면 발동
        if (IsCooldownReady() && CanFire())
        {
            Active();
            _lastActiveTime = Time.time;
        }
    }
    private void Targeting()
    {
        // 조준형일 때만 작동
        if (_targetingType == TARGETING_TYPE.Closest)
        {
            // 공격 방향
            Vector2 attackDir = SkillManager.Instance.Player.AttackDir;

            // 공격 방향이 존재하면
            if (attackDir.sqrMagnitude > 0f)
            {
                // 코루틴이 안 돌고 있다면 켜기
                if (_searchCoroutine == null)
                    _searchCoroutine = StartCoroutine(SearchTargetCoroutine());
            }
            else
            {
                // 공격 방향 없으면
                // 코루틴 끄고 타겟 비우기
                StopSearch();
            }
        }
    }

    // 탐색 중지
    private void StopSearch()
    {
        // 코루틴 돌고있으면 중지
        if (_searchCoroutine != null)
        {
            StopCoroutine(_searchCoroutine);
            _searchCoroutine = null;
        }

        // 타겟 비우기
        _currentTarget = null;
    }

    // 타겟 탐색 코루틴
    private IEnumerator SearchTargetCoroutine()
    {
        while (true)
        {
            // 공격 방향
            Vector2 attackDir = SkillManager.Instance.Player.AttackDir;

            // 방향 있을 때 탐색
            if (attackDir.sqrMagnitude > 0f)
                _currentTarget = SkillManager.Instance.TargetingSystem.GetTarget(attackDir, _distance, _angle);
            else
                _currentTarget = null;

            // 탐색 0.2초 대기
            yield return SkillManager.Instance.SearchInterval;
        }
    }

    // 쿨타임 체크
    private bool IsCooldownReady()
    {
        float cooldown = Mathf.Max(MIN_SKILL_COOLDOWN, _finalStat.Cooldown);
        return Time.time >= _lastActiveTime + cooldown;
    }

    protected virtual bool CanFire()
    {
        var player = SkillManager.Instance.Player;

        switch (_targetingType)
        {
            case TARGETING_TYPE.Barrier:                    // 베리어형     항상 발동
                return true;

            case TARGETING_TYPE.NonTarget:                  // 비대상형    공격 방향 있어야 발동
                return player.AttackDir.sqrMagnitude > 0f;

            case TARGETING_TYPE.Closest:                    // 조준형      공격방향, 타겟 트랜스폼 둘 다 있어야 발동
                return player.AttackDir.sqrMagnitude > 0f && _currentTarget != null;

            case TARGETING_TYPE.Directional:                // 이동방향형  이동 방향 있어야 발동
                return player.MoveDir.sqrMagnitude > 0f;

            default:
                return true;
        }
    }

    // 자식에서 쿨타임, 조건 변경 해야하면 이걸로 오버라이드
    protected virtual bool CanActive()
    {
        return IsCooldownReady() && CanFire();
    }

    // 스킬 발동 로직
    private void Active()
    {
        StartCoroutine(ActiveCoroutine());
    }

    // 투사체 수만큼 순차 발사
    protected virtual IEnumerator ActiveCoroutine()
    {
        // 투사체 수
        int count = Mathf.Max(1, _finalStat.ProjectileCount);

        for (int i = 0; i < count; i++)
        {
            // 실제 발사 로직은 자식에서
            OnActive(i, count);

            // 발사 텀
            yield return _fireDelay;
        }
    }
    
    // 자식 개별 
    protected abstract void OnActive(int index, int totalCount);



    // 스킬 업그레이드
    public virtual void ApplyUpgrade(ActiveUpgradeData upgradeData)
    {
        AddSubTag(upgradeData);         // 업그레이드의 서브 태그 추가
        LevelUp(upgradeData);           // 스킬 레벨, 업그레이드 레벨 증가
        AddStat(upgradeData);           // 공용, 업그레이드 스탯 누적
        AddModifier(upgradeData);       // 전용 모디파이어 추가

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
    private void AddModifier(ActiveUpgradeData upgradeData)
    {
        // id에 맞는 전용 모디파이어 가져오기
        ActiveModifier modifier = SkillDataLoader.GetActiveModifier(upgradeData.Id);
        
        if (modifier != null)
        {
            // 이미 있는 모디파이어는 스킵
            if (_skillModifiers.Exists(pair => pair.upgradeData.Id == upgradeData.Id)) return;

            // 모디파이어 추가
            _skillModifiers.Add((modifier, upgradeData));
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
        //Debug.Log($"최종 공격력 : {resultStat.Damage} = (( {baseStat.Damage}(기본) + 패시브 깡 스탯) * {commonStat.Damage}(공용)  + ({upgradeStat.Damage}(업그레이드) * 추가추가피해)) * {passiveMulStat.Damage}(패시브 배율) * 패시브 최종 배율");
        //Debug.Log($"최종 지속시간 : {resultStat.Duration} = (( {baseStat.Duration}(기본) + 패시브 깡 스탯) * {commonStat.Duration}(공용)  + ({upgradeStat.Duration}(업그레이드) * 추가추가피해)) * {passiveMulStat.Duration}(패시브 배율) * 패시브 최종 배율");
        //Debug.Log($"최종 틱 주기 : {resultStat.TickRate} = (( {baseStat.TickRate}(기본) + 패시브 깡 스탯) * {commonStat.TickRate}(공용)  + ({upgradeStat.TickRate}(업그레이드) * 추가추가피해)) * {passiveMulStat.TickRate}(패시브 배율) * 패시브 최종 배율");
        //Debug.Log($"최종 투사체 수 : {resultStat.ProjectileCount} = (( {baseStat.ProjectileCount}(기본) + 패시브 깡 스탯) * {commonStat.ProjectileCount}(공용)  + ({upgradeStat.ProjectileCount}(업그레이드) * 추가추가피해)) * {passiveMulStat.ProjectileCount}(패시브 배율) * 패시브 최종 배율");
        //Debug.Log($"최종 넉백 : {resultStat.Knockback} = (( {baseStat.Knockback}(기본) + 패시브 깡 스탯) * {commonStat.Knockback}(공용)  + ({upgradeStat.Knockback}(업그레이드) * 추가추가피해)) * {passiveMulStat.Knockback}(패시브 배율) * 패시브 최종 배율");

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
    public virtual void RecheckModifiers()
    {
        SetPassiveMulStat();    // 패시브 배율 합
        AddPassiveModifier();   // 패시브 모디파이어
        CalculateStats();       // finalStat 계산
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

    // 풀 매니저의 GameObject -> 컴포넌트로 캐시 (GetComponent 줄이기)
    private Dictionary<GameObject, Component> _component = new();

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

        foreach (var pair in _skillModifiers)
        {
            // 모디파이어 적용
            pair.modifier.Apply(this, pair.upgradeData);
        }

        Debug.Log($"[ActiveSkill] 업그레이드 완료: [{SkillDataLoader.GetActiveSkillData(MainTag).SkillName}] (MainTag : {MainTag})\n" +
                  $" - 업그레이드 : {upgradeData.Name} (Active_Skill_ID : {upgradeData.Id})\n" +
                  $" - 업그레이드 Lv : {GetUpgradeLevel(upgradeData.Id)} / {upgradeData.MaxLevel}\n" +
                  $" - 스킬 전체 Lv : {CurrentLevel} / {MAX_SKILL_LEVEL}");
    }


    // 메인 프리팹 풀에서 꺼내기
    protected TSkillObject SpawnFromPool(string tag, Vector3 pos, Quaternion rot)
    {
        return SpawnFromPool<TSkillObject>(tag, pos, rot);
    }

    // 메인, 서브 프리팹 풀에서 꺼내기
    protected T SpawnFromPool<T>(string tag, Vector3 pos, Quaternion rot) where T : Component
    {
        // 풀 매니저 체크
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogWarning("[ActiveSkill] ObjectPoolManager 없음");
            return null;
        }

        // 풀 매니저에서 tag의 프리팹 가져오기
        GameObject obj = ObjectPoolManager.Instance.Get(tag, pos, rot);

        // 프리팹 null
        if (obj == null)
        {
            Debug.LogWarning($"[ActiveSkill] 풀 미등록 태그: {tag}");
            return null;
        }

        // 캐시에 있으면 바로 반환
        if (_component.TryGetValue(obj, out Component cached) && cached is T result)
            return result;

        // 없으면 GetComponent 후 캐싱
        T comp = obj.GetComponent<T>();
        _component[obj] = comp;

        return comp;
    }
}
