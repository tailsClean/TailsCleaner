using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 탑 선택 캐러셀 UI (세로 버전)
///
/// 레이아웃 (앵커 정중앙 기준):
///   이전 탑 : 1000×200, anchoredPosition (0, 220)
///   선택 탑 : 1000×200, anchoredPosition (0, 0)
///   다음 탑 : 1000×200, anchoredPosition (0, -220)
/// </summary>
public class DungeonSelectVertical : MonoBehaviour
{
    [Header("슬롯 RectTransform")]
    [SerializeField] private RectTransform _slotPrev;
    [SerializeField] private RectTransform _slotSelected;
    [SerializeField] private RectTransform _slotNext;

    [Header("버튼")]
    [SerializeField] private Button _btnPrev;
    [SerializeField] private Button _btnNext;
    [SerializeField] private Button _btnSelect;
    [SerializeField] private Button _btnMain;
    [SerializeField] private Button _btnSetting;

    [Header("텍스트")]
    [SerializeField] private TextMeshProUGUI _txtSelect;
    [SerializeField] private TextMeshProUGUI _txtPrev;
    [SerializeField] private TextMeshProUGUI _txtNext;

    [Header("데이터")]
    private TowerTableSO _towerData;
    private StringSO _stringData;

    [Header("애니메이션")]
    [SerializeField] private float _moveDuration = 0.25f;
    [SerializeField] private Ease _moveEase = Ease.OutCubic;

    private static readonly Vector2 POS_PREV     = new Vector2(0, 220);
    private static readonly Vector2 POS_SELECTED = new Vector2(0, 0);
    private static readonly Vector2 POS_NEXT     = new Vector2(0, -220);
    private static readonly Vector2 SIZE_SLOT    = new Vector2(1000, 200);

    private int _currentIndex = 0;
    private int TowerCount => _towerData != null ? _towerData.dataList.Count : 0;

    private void Start()
    {
        _btnPrev.onClick.AddListener(OnPrev);
        _btnNext.onClick.AddListener(OnNext);
        _btnSelect.onClick.AddListener(OnSelect);
        _btnMain.onClick.AddListener(OnGoToMain);
        _btnSetting.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);

        _towerData = DataManager.Instance.GetSOData<TowerTableSO>();
        _stringData = DataManager.Instance.GetSOData<StringSO>();
        Refresh(animate: false);
    }

    private void OnDestroy()
    {
        _slotPrev.DOKill();
        _slotSelected.DOKill();
        _slotNext.DOKill();
    }

    private void OnPrev()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        Refresh(animate: true);
    }

    private void OnNext()
    {
        if (_currentIndex >= TowerCount - 1) return;
        _currentIndex++;
        Refresh(animate: true);
    }

    private void OnSelect()
    {
        GameManager.Instance._currentTower = _towerData.dataList[_currentIndex];
        this.gameObject.SetActive(false);
        UIManager.Instance.ChangeStateStageSelect();
    }

    private void OnGoToMain()
    {
        this.gameObject.SetActive(false);
    }

    private void Refresh(bool animate)
    {
        bool hasPrev = _currentIndex > 0;
        bool hasNext = _currentIndex < TowerCount - 1;

        _slotPrev.gameObject.SetActive(hasPrev);
        _slotNext.gameObject.SetActive(hasNext);

        _btnPrev.interactable = hasPrev;
        _btnNext.interactable = hasNext;

        BindText(_txtSelect,  _currentIndex);
        if (hasPrev) BindText(_txtPrev,  _currentIndex - 1);
        if (hasNext) BindText(_txtNext,  _currentIndex + 1);

        AnimateSlot(_slotSelected, POS_SELECTED, SIZE_SLOT, animate);
        if (hasPrev) AnimateSlot(_slotPrev, POS_PREV, SIZE_SLOT, animate);
        if (hasNext) AnimateSlot(_slotNext, POS_NEXT, SIZE_SLOT, animate);
    }

    private void BindText(TextMeshProUGUI txt, int index)
    {
        if (_towerData == null || index < 0 || index >= TowerCount) return;

        var data = _towerData.dataList[index];
        var imgName = data.tower_icon_resource;
        var towerName = _stringData.GetById(data.tower_string_key).kr;
        txt.text = towerName;
    }

    private void AnimateSlot(RectTransform rt, Vector2 targetPos, Vector2 targetSize, bool animate)
    {
        rt.DOKill();

        if (!animate)
        {
            rt.anchoredPosition = targetPos;
            rt.sizeDelta = targetSize;
            return;
        }

        rt.DOAnchorPos(targetPos, _moveDuration).SetEase(_moveEase);
        rt.DOSizeDelta(targetSize, _moveDuration).SetEase(_moveEase);
    }
}

