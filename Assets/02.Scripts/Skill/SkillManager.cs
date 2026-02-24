using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelUpSelect))]
public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    public const int MAX_ACTIVE_SLOTS = 6;              // 최대 액티브 스킬 수
    public const int MAX_PASSIVE_SLOTS = 6;             // 최대 패시브 스킬 수
    public const int DEFAULT_ACTIVE_MAIN_TAG = 41002;   // 기본 지급 스킬 메인 태그 (비누 던지기)
    public const float DEFAULT_SEARCH_RADIUS = 20f;     // 가장 가까운 적 탐색용 범위
    public const float SEARCH_INTERVAL = 0.2f;          // 탐색 주기

    public WaitForSeconds SearchInterval { get; } = new WaitForSeconds(SEARCH_INTERVAL);

    // 플레이어 보유 스킬 리스트
    public List<ActiveSkill> MyActiveSkills { get; private set; } = new();
    public List<PassiveSkillData> MyPassiveSkills { get; private set; } = new();


    // 스킬 슬롯 체크
    public bool IsActiveSlotFull => MyActiveSkills.Count >= MAX_ACTIVE_SLOTS;
    public bool IsPassiveSlotFull => MyPassiveSkills.Count >= MAX_PASSIVE_SLOTS;


    public PlayerBase Player { get; private set; }

    [Header("적 탐색 레이어")]
    [SerializeField] LayerMask _monsterLayer; 



    private void Awake() { Instance = this; Player = GetComponent<PlayerBase>(); }

    private void Start()
    {
        // 스킬 데이터 불러오기
        SkillDataLoader.Init();

        // 기본 스킬 추가
        // 기획서 상에서는 못봤는데 기본 공격이 없으면 공격 못하니까 일단 추가
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

    // 메인 태그의 0티어 반환
    public ActiveUpgradeData GetTierZeroData(int mainTag)
    {
        // 해당 스킬의 업그레이드 리스트 가져오기
        var list = SkillDataLoader.GetActiveUpgradeDatas(mainTag);

        // 리스트에서 0티어 찾아 반환
        return list.Find(data => data.Tier == 0);
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

    #region 선택지 적용

    // 액티브 선택지 적용
    public void ApplyActiveOption(int targetMainTag, ActiveUpgradeData upgradeData)
    {
        // 0티어 신규 생성
        if (upgradeData.Tier == 0)
        {
            // 액티브 데이터
            ActiveSkillData skillData = SkillDataLoader.GetActiveSkillData(targetMainTag);

            // 스킬 프리팹을 자식으로 생성
            ActiveSkill newSkill = Instantiate(skillData.SkillPrefab, transform);

            // 스킬 투사체 프리팹
            GameObject projectile = skillData.SkillProjectilePrefab;

            // 위치 초기화
            newSkill.transform.localPosition = Vector2.zero;

            // 초기화
            newSkill.Init(skillData, upgradeData, projectile);

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
            ActiveSkill skill = GetActiveSkill(targetMainTag);

            // 유효하면 업그레이드 데이터 적용
            if (skill != null)
            {
                skill.ApplyUpgrade(upgradeData);
            }
        }
    }

    // 패시브 선택지 적용
    public void ApplyPassiveOption(PassiveSkillData passiveData)
    {
        if (passiveData == null) return;

        // 패시브에 추가
        MyPassiveSkills.Add(passiveData);

        // 모든 액티브 스킬에
        foreach (var skill in MyActiveSkills)
        {
            // 스탯 계산 + 로직 적용
            skill.RecheckPassives();
        }

        Debug.Log($"[SkillManager] 패시브 획득: {passiveData.PassiveName} (SubTag: {passiveData.SubTag})");
    }

    #endregion


    #region 초기화

    // 시작 기본 스킬
    private void AddDefaultSkill()
    {
        // 메인태그의 0번 업그레이드 가져오기 (0티어 스킬 그자체)
        ActiveUpgradeData defualtSkillData = SkillDataLoader.GetActiveUpgradeDatas(DEFAULT_ACTIVE_MAIN_TAG)[0];

        if (defualtSkillData != null)
        {
            Debug.Log($"기본 스킬 {defualtSkillData.Name} ({DEFAULT_ACTIVE_MAIN_TAG}) 지급");
            ApplyActiveOption(DEFAULT_ACTIVE_MAIN_TAG, defualtSkillData);
        }
        else
        {
            Debug.Log($"{DEFAULT_ACTIVE_MAIN_TAG}의 0티어가 존재하지 않음");
        }
    }

    #endregion



    // 공용 적 탐색
    public MonsterBase FindClosestMonster(Transform origin, float radius = DEFAULT_SEARCH_RADIUS)
    {
        // 범위 내 몬스터 레이어 수집
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin.position, radius, _monsterLayer);

        MonsterBase closest = null;
        float minSqrDist = float.MaxValue;

        foreach (var hit in hits)
        {
            // 몬스터 베이스 없으면 패스
            if (hit.TryGetComponent<MonsterBase>(out var monster) == false) continue;

            // sqrMagnitude 사용으로 sqrt 회피
            float sqrDist = ((Vector2)origin.position - (Vector2)hit.transform.position).sqrMagnitude;

            // 최소 거리 갱신
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closest = monster;
            }
        }

        return closest;
    }
}
