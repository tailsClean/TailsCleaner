using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPopupUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StringSO _stringSO;
    [SerializeField] private int _startStringId = 257;

    [Header("Root")]
    [SerializeField] private GameObject _popupRoot;

    [Header("UI")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;
    [SerializeField] private Button _closeButton;

    [Header("Tab Buttons (257 ~ 260 순서대로)")]
    [SerializeField] private Button[] _tabButtons;

    [Header("Tab Titles (257 ~ 260 순서대로)")]
    [SerializeField] private string[] _tabTitles;

    private void Awake()
    {
        BindButtons();

        if (_popupRoot != null)
            _popupRoot.SetActive(false);
    }

    private void BindButtons()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(Close);
        }

        if (_tabButtons == null)
            return;

        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int capturedIndex = i;

            if (_tabButtons[i] == null)
                continue;

            _tabButtons[i].onClick.RemoveAllListeners();
            _tabButtons[i].onClick.AddListener(() => ShowTab(capturedIndex));
        }
    }

    public void Open()
    {
        if (_popupRoot != null)
            _popupRoot.SetActive(true);

        ShowTab(0);
    }

    public void Close()
    {
        if (_popupRoot != null)
            _popupRoot.SetActive(false);
    }

    public void ShowTab(int tabIndex)
    {
        if (tabIndex < 0 || tabIndex > 3)
        {
            Debug.LogWarning($"[HelpPopupUI] Invalid tab index: {tabIndex}");
            return;
        }

        int stringId = _startStringId + tabIndex;
        string stringIdText = stringId.ToString();

        string title = GetTitle(tabIndex);
        string body = GetBody(stringIdText);

        if (_titleText != null)
            _titleText.text = title;

        if (_bodyText != null)
            _bodyText.text = body;

        Debug.Log($"[HelpPopupUI] ShowTab index={tabIndex}, stringId={stringIdText}");
    }

    private string GetTitle(int tabIndex)
    {
        if (_tabTitles != null && tabIndex >= 0 && tabIndex < _tabTitles.Length)
        {
            if (!string.IsNullOrEmpty(_tabTitles[tabIndex]))
                return _tabTitles[tabIndex];
        }

        return $"도움말 {_startStringId + tabIndex}";
    }

    private string GetBody(string stringId)
    {
        if (_stringSO == null)
        {
            Debug.LogWarning("[HelpPopupUI] StringSO is null.");
            return $"StringSO가 연결되지 않았습니다. (id={stringId})";
        }

        var data = _stringSO.GetById(stringId);
        if (data == null)
        {
            Debug.LogWarning($"[HelpPopupUI] String data not found. id={stringId}");
            return $"해당 도움말 데이터를 찾을 수 없습니다. (id={stringId})";
        }

        return data.kr;
    }
}