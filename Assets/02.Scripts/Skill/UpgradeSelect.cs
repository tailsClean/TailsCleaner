using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UpgradeSelect : MonoBehaviour
{
    public const int MAX_SELECT_OPTIONS = 3;      // 최대 선택지 수
    public const int ACTIVE_TIER_TWO_LEVEL = 4;   // 티어 2 레벨
    public const int ACTIVE_TIER_THREE_LEVEL = 7; // 티어 3 레벨

    // 현재 선택지
    private List<ActiveUpgradeData> _currentOptions = new List<ActiveUpgradeData>();

    // 선택지 옵션 설정
    public void GenerateOptions()
    {
        _currentOptions.Clear();

        var sm = SkillManager.Instance;

        // 상태 체크 (액티브 최대, 레벨 최대)
        bool isActiveFull = sm.IsActiveSlotFull;
        bool isAllLevelMax = sm.IsAllActiveMaxLevel();

        // 두 조건 중 하나라도 아닐 때
        // 액티브 스킬 후보 생성
        if (isActiveFull == false || isAllLevelMax == false)
        {
            // 메인 태그 후보 추리기 (0티어)
            List<int> candidateMainTags = GetCandidateMainTags(sm);

            // 추려진 메인 태그 순회
            foreach (int mainTag in candidateMainTags)
            {
                // 선택지 꽉 찼으면 중단
                if (_currentOptions.Count >= MAX_SELECT_OPTIONS) break;

                // 메인 태그로 선택지 뽑기
                var option = PickOptionForTag(sm, mainTag);

                // 유효하면 추가
                if (option != null) _currentOptions.Add(option);
            }
        }

        // 액티브 체크 했는데 선택지 남았으면
        if (_currentOptions.Count < MAX_SELECT_OPTIONS)
        {
            // 패시브 추가 로직
        }

        // 패시브에도 자리가 없다면
        if (_currentOptions.Count < MAX_SELECT_OPTIONS)
        {
            // 회복 추가 로직
        }

        // UI 없으니 임시 로그
        PrintLog();
    }

    // 테스트용 선택지 로그
    private void PrintLog()
    {
        Debug.Log("[UpgradeSelect] 선택지 생성");
        for (int i = 0; i < _currentOptions.Count; i++)
        {
            Debug.Log($"{i + 1}. {_currentOptions[i].Name}");
        }
    }


    // 메인 태그 후보 추리기
    private List<int> GetCandidateMainTags(SkillManager sm)
    {
        // 임시 태그 후보 리스트
        List<int> tempMainTags = new List<int>();

        // 보유 중인 액티브 스킬 중
        for (int i = 0; i < sm.MyActiveSkills.Count; i++)
        {
            // 최대 레벨 아닌 스킬 후보에 추가
            if (sm.MyActiveSkills[i].CurrentLevel < ActiveSkill.MAX_SKILL_LEVEL)
                tempMainTags.Add(sm.MyActiveSkills[i].MainTag);
        }

        // 미보유 스킬 (슬롯 남으면)
        if (sm.MyActiveSkills.Count < SkillManager.MAX_ACTIVE_SLOTS)
        {
            // 전체 업그레이드 데이터 순회
            for (int i = 0; i < sm.AllActiveUpgradeData.Count; i++)
            {
                var data = sm.AllActiveUpgradeData[i];

                // 0티어 && 미습득 && 미중복
                if (data.Tier == 0 && sm.GetActiveSkill(data.MainTag) == null && tempMainTags.Contains(data.MainTag) == false)
                {
                    // 후보에 추가
                    tempMainTags.Add(data.MainTag);
                }
            }
        }

        // 셔플
        return Shuffle(tempMainTags);
    }

    // 셔플 (Fisher-Yates)
    private List<int> Shuffle(List<int> tempMainTags)
    {
        for (int i = tempMainTags.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            int temp = tempMainTags[i];
            tempMainTags[i] = tempMainTags[rand];
            tempMainTags[rand] = temp;
        }

        return tempMainTags;
    }

    private ActiveUpgradeData PickOptionForTag(SkillManager sm, int mainTag)
    {
        // 보유한 메인 태그 스킬 가져오기
        ActiveSkill mySkill = sm.GetActiveSkill(mainTag);

        // 없으면 신규 스킬 (0티어)
        if (mySkill == null)
        {
            return sm.FindUpgradeData(mainTag, 0);
        }

        // 보유 스킬 (티어 계산)
        int level = mySkill.CurrentLevel;
        List<int> tiers = new List<int>();

        // 레벨 별 티어
        tiers.Add(1);
        if (level >= ACTIVE_TIER_TWO_LEVEL) tiers.Add(2);
        if (level >= ACTIVE_TIER_THREE_LEVEL) tiers.Add(3);

        // 임시 업그레이드 후보
        List<ActiveUpgradeData> candidates = new List<ActiveUpgradeData>();

        // 전체에서 조건에 맞는 데이터 필터링
        for (int i = 0; i < sm.AllActiveUpgradeData.Count; i++)
        {
            var data = sm.AllActiveUpgradeData[i];

            // 태그 일치 && 티어 포함
            if (data.MainTag == mainTag && tiers.Contains(data.Tier))
            {
                // 업그레이드 최대 레벨인지 체크
                if (mySkill.GetUpgradeLevel(data.Id) < data.MaxLevel)
                {
                    // 아니면 후보에 추가
                    candidates.Add(data);
                }
            }
        }

        // 하나라도 있으면 그 중 랜덤
        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        // 조건에 맞는 업그레이드 없으면
        return null;
    }



    // 임시 테스트용 선택지 선택
    public void SelectOption(int index)
    {
        // 비정상 범위 체크
        if (index < 0 || index >= _currentOptions.Count) return;

        ActiveUpgradeData data = _currentOptions[index];

        // 매니저에서 선택지 적용
        SkillManager.Instance.ApplyOption(data);

        _currentOptions.Clear();
    }


   // 선택지 테스트용 인풋
   private  void Update()
    {
        // 스페이스바 선택지 생성
        if (Keyboard.current.spaceKey.wasPressedThisFrame == true)
            GenerateOptions();

        // 숫자키 선택 및 적용
        if (_currentOptions.Count > 0)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectOption(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectOption(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectOption(2);
        }
    }
}
