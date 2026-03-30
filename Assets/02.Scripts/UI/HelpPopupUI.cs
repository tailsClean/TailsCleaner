using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPopupUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private StringSO _stringSO;

    [Header("UI")]
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _bodyText;

    [Header("Tab Buttons")]
    [SerializeField] private Button[] _tabButtons;

    [Header("Tab Titles")]
    [SerializeField] private string[] _tabTitles;

    [Header("Tab String IDs")]
    [SerializeField] private string[] _tabStringIds;

    [Header("Tab Button Images")]
    [SerializeField] private Image[] _tabButtonImages;
    [SerializeField] private Sprite _selectedSprite;
    [SerializeField] private Sprite _unselectedSprite;

    private void Awake()
    {
        BindTabButtons();
    }

    private void BindTabButtons()
    {
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
        gameObject.SetActive(true);
        ShowTab(0);
        Debug.Log("[HelpPopupUI] Open");
    }

    public void Close()
    {
        gameObject.SetActive(false);
        Debug.Log("[HelpPopupUI] Close");
    }

    public void Toggle()
    {
        bool nextActive = !gameObject.activeSelf;
        gameObject.SetActive(nextActive);

        if (nextActive)
        {
            ShowTab(0);
            Debug.Log("[HelpPopupUI] Toggle -> Open");
        }
        else
        {
            Debug.Log("[HelpPopupUI] Toggle -> Close");
        }
    }

    public void ShowTab(int index)
    {
        if (_tabStringIds == null || index < 0 || index >= _tabStringIds.Length)
        {
            Debug.LogWarning($"[HelpPopupUI] Invalid tab index: {index}");
            return;
        }

        string stringId = _tabStringIds[index];

        if (_titleText != null)
        {
            if (_tabTitles != null && index < _tabTitles.Length)
                _titleText.text = _tabTitles[index];
            else
                _titleText.text = stringId;
        }

        if (_bodyText != null)
        {
            _bodyText.text = GetBodyText(stringId);
        }

        RefreshTabVisual(index);

        Debug.Log($"[HelpPopupUI] ShowTab {index}, stringId={stringId}");
    }

    private string GetBodyText(string stringId)
    {
        if (_stringSO == null)
        {
            Debug.LogWarning("[HelpPopupUI] StringSO is null.");
            return "StringSO가 연결되지 않았습니다.";
        }

        var data = _stringSO.GetById(stringId);
        if (data == null)
        {
            Debug.LogWarning($"[HelpPopupUI] String data not found. id={stringId}");
            return $"데이터 없음 ({stringId})";
        }

        // 현재 StringSO.asset 구조 기준으로는 kr 필드가 맞을 가능성이 높음
        return data.kr;
    }

    private void RefreshTabVisual(int selectedIndex)
    {
        if (_tabButtonImages == null || _tabButtonImages.Length == 0)
            return;

        for (int i = 0; i < _tabButtonImages.Length; i++)
        {
            if (_tabButtonImages[i] == null)
                continue;

            if (_selectedSprite != null && _unselectedSprite != null)
            {
                _tabButtonImages[i].sprite = (i == selectedIndex)
                    ? _selectedSprite
                    : _unselectedSprite;
            }
            else
            {
                _tabButtonImages[i].color = (i == selectedIndex)
                    ? new Color(1f, 1f, 1f, 1f)
                    : new Color(0.75f, 0.75f, 0.75f, 1f);
            }
        }
    }
}