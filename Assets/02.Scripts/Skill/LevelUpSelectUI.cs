using UnityEngine;
using UnityEngine.UI;

public class LevelUpSelectUI : MonoBehaviour
{
    [Header("보유 스킬 아이콘")]
    [SerializeField] Image[] ActiveSkillIcons;
    [SerializeField] Image[] PassiveSkillIcons;

    [Header("선택지 버튼")]
    public SelectOptionButton[] OptionButtons;

    [Header("레벨업 이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onLevelUp;

    [Header("체력 회복 선택지 설정")]
    [SerializeField] string _healName = "체력 회복";
    [SerializeField] string _healDesc = "최대 체력의 30%를 회복한다";
    [SerializeField] Sprite _healIcon;

    private LevelUpSelect _levelUpSelect;

    private void Start()
    {
        _levelUpSelect = SkillManager.Instance.GetComponent<LevelUpSelect>();

        // 선택지 버튼 이벤트 설정
        for (int i = 0; i < OptionButtons.Length; i++)
        { 
            // 클로저 이슈 방지
            int captureIndex = i;

            OptionButtons[i].Init(() => OnOptionClick(captureIndex));
        }

        // 레벨업 채널 구독
        _onLevelUp.AddListener(LevelUp);

        // 설정 다 하고 끄기
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _onLevelUp.RemoveListener(LevelUp);
    }

    // 레벨 업 시 호출
    public void LevelUp(int level)
    {
        Debug.Log("레벨업");

        gameObject.SetActive(true);

        // 상단 보유 스킬 아이콘 갱신
        UpdateSkillIcons(); 

        // 새로운 선택지 생성
        _levelUpSelect.GenerateOptions();

        // 생성된 선택지 데이터 버튼들에 채워넣기
        UpdateOptionButtons();

        // 게임 일시정지
        Time.timeScale = 0f;
    }


    // 상단 보유 스킬 아이콘 갱신
    private void UpdateSkillIcons()
    {
        // 보유 액티브 스킬
        var activeSkills = SkillManager.Instance.MyActiveSkills;

        for (int i = 0; i < ActiveSkillIcons.Length; i++)
        {
            if (i < activeSkills.Count)
            {
                // ActiveSkillData에 나중에 Sprite Icon 추가
                // ActiveSkillIcons[i].sprite = SkillDataLoader.GetActiveSkillData(activeSkills[i].MainTag).Icon;
                // 일단 하얗게
                ActiveSkillIcons[i].color = Color.white;
            }
            else
            {
                // 빈 칸은 빈 스프라이트인데 일단 회색
                ActiveSkillIcons[i].color = Color.gray;
            }
        }

        // 보유 패시브 스킬
        var passiveSkills = SkillManager.Instance.MyPassiveSkills;
        for (int i = 0; i < PassiveSkillIcons.Length; i++)
        {
            if (i < passiveSkills.Count)
            {
                // PassiveSkillData도 Icon 필요
                // PassiveSkillIcons[i].sprite = passiveSkills[i].Icon;
                PassiveSkillIcons[i].color = Color.white;
            }
            else
            {
                PassiveSkillIcons[i].color = Color.gray;
            }
        }
    }

    // 선택지 버튼 갱신
    private void UpdateOptionButtons()
    {
        // 현재 선택지
        var options = _levelUpSelect.CurrentOptions;

        for (int i = 0; i < OptionButtons.Length; i++)
        {
            OptionButtons[i].gameObject.SetActive(true);
            var optionData = options[i];

            string name = "";
            string desc = "";
            Sprite icon = null;

            // 타입에 맞게
            switch (optionData.Type)
            {
                case LevelUpSelect.OPTION_TYPE.Active:      // 액티브
                    name = optionData.ActiveData.Name;
                    desc = optionData.ActiveData.Desc;
                    // icon = optionData.ActiveData.Icon;
                    break;

                case LevelUpSelect.OPTION_TYPE.Passive:     // 패시브
                    name = optionData.PassiveData.PassiveName;
                    desc = optionData.PassiveData.Desc;
                    // icon = optionData.PassiveData.Icon;
                    break;

                case LevelUpSelect.OPTION_TYPE.Heal:        // 회복
                    name = _healName;
                    desc = _healDesc;
                    icon = _healIcon;
                    break;
            }

            // 선택지 내용 설정
            OptionButtons[i].Setup(name, desc, icon);
        }
    }

    // 버튼 클릭 시
    private void OnOptionClick(int index)
    {
        // 스킬 적용
        _levelUpSelect.SelectOption(index);

        // UI 닫고 게임 재개
        CloseUI();
    }

    private void CloseUI()
    {
        // ui 끄고
        gameObject.SetActive(false);
        // 게임 다시 시작
        Time.timeScale = 1f;
    }
}
