using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageUIContainer : MonoBehaviour, UIContainer // stageUI에서 연결되어야 할 UI 요소들을 참조하는 클래스
{
    [SerializeField] private Button _settingButton;
    [SerializeField] private GameObject _exitPanel;
    public GameObject ExitPanel => _exitPanel;
    [SerializeField] private GameObject _timerUI;
    public GameObject TimerUI => _timerUI;
    [SerializeField] private GameObject _gameOverPanel;
    public GameObject GameOverPanel => _gameOverPanel;
    [SerializeField] private GameObject _stageClearPanel;
    public GameObject StageClearPanel => _stageClearPanel;


    [SerializeField] private List<UIGroup> _uiGroupList;
    public Dictionary<UIGroup.UISTATE, UIGroup> _uiDict;


    private void Awake()
    {
        _uiDict = new Dictionary<UIGroup.UISTATE, UIGroup>();
        foreach (var uiGroup in _uiGroupList)
        {
            _uiDict.Add(uiGroup.UIState, uiGroup);
        }
    }

    void Start()
    {
        _settingButton.onClick.AddListener(() => {
        UIManager.Instance.ChangeStateExitPanel();
    });
        
    }

    public void SetActiveUiGroup(UIGroup.UISTATE uiState, bool active)
    {
        if (_uiDict.TryGetValue(uiState, out var uIGroup))
            uIGroup.gameObject.SetActive(active);
        else
            Debug.LogWarning(uiState + "에 해당하는 UI그룹이 없습니다.");
    }
}
