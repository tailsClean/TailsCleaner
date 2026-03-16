using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
/// <summary>
/// 탑 선택 캐러셀 UI
///
/// 레이아웃 (앵커 정중앙 기준):
///   이전 탑 : 300×480, anchoredPosition (-620, 0)
///   선택 탑 : 380×600, anchoredPosition (-245, 0)
///   다음 탑 : 300×480, anchoredPosition ( 130, 0)
///
/// 예외
///   - 0번째 선택 시 이전 슬롯 비움
///   - 마지막 선택 시 다음 슬롯 비움
/// </summary>

public class DungeonSelect : MonoBehaviour
{
    [Header("슬롯 RectTransform")]
    [SerializeField] private RectTransform _slotPrev;
    [SerializeField] private RectTransform _slotSelected;
    [SerializeField] private RectTransform _slotNext;
 
    [Header("버튼")]
    [SerializeField] private Button _btnPrev;
    [SerializeField] private Button _btnNext;
    [SerializeField] private Button _btnSelect;
 
    [Header("데이터")]
     private TowerTableSO _towerData;
 
    [Header("애니메이션")]
    [SerializeField] private float    _moveDuration = 0.25f;
    [SerializeField] private Ease     _moveEase     = Ease.OutCubic;
    
    private static readonly Vector2 POS_PREV     = new Vector2(-620, 0);
    private static readonly Vector2 POS_SELECTED = new Vector2(-245, 0);
    private static readonly Vector2 POS_NEXT     = new Vector2( 130, 0);
    private static readonly Vector2 SIZE_SMALL   = new Vector2(300, 480);
    private static readonly Vector2 SIZE_LARGE   = new Vector2(380, 600);
 
    private int _currentIndex = 0;
    private int TowerCount => _towerData != null ? _towerData.dataList.Count : 0;
 
    private void Start()
    {
        _btnPrev.onClick.AddListener(OnPrev);
        _btnNext.onClick.AddListener(OnNext);
        _btnSelect.onClick.AddListener(OnSelect);
        _towerData = DataManager.Instance.GetSOData<TowerTableSO>();
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
    }
 
    private void Refresh(bool animate)
    {
        bool hasPrev = _currentIndex > 0;
        bool hasNext = _currentIndex < TowerCount - 1;
 
        _slotPrev.gameObject.SetActive(hasPrev);
        _slotNext.gameObject.SetActive(hasNext);
 
        _btnPrev.interactable = hasPrev;
        _btnNext.interactable = hasNext;
 
        BindSlot(_slotSelected, _currentIndex);
        if (hasPrev) BindSlot(_slotPrev, _currentIndex - 1);
        if (hasNext) BindSlot(_slotNext, _currentIndex + 1);
 
        AnimateSlot(_slotSelected, POS_SELECTED, SIZE_LARGE,  animate);
        if (hasPrev) AnimateSlot(_slotPrev,  POS_PREV,  SIZE_SMALL, animate);
        if (hasNext) AnimateSlot(_slotNext,  POS_NEXT,  SIZE_SMALL, animate);
    }
 
    private void BindSlot(RectTransform slot, int index)
    {
        if (_towerData == null || index < 0 || index >= TowerCount) return;
 
        var data = _towerData.dataList[index];
    }

    private void AnimateSlot(RectTransform rt, Vector2 targetPos, Vector2 targetSize, bool animate)
    {
        rt.DOKill();
 
        if (!animate)
        {
            rt.anchoredPosition = targetPos;
            rt.sizeDelta        = targetSize;
            return;
        }
 
        rt.DOAnchorPos(targetPos,  _moveDuration).SetEase(_moveEase);
        rt.DOSizeDelta(targetSize, _moveDuration).SetEase(_moveEase);
    }
}


    

