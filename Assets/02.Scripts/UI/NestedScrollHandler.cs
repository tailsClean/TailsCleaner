using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedScrollHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ScrollRect _parentScroll; // 세로
    [SerializeField] private ScrollRect _childScroll;  // 가로

    private ScrollRect _activeScroll;
    private bool _isHorizontalDrag;
    private bool _isDragging;
    private Vector2 _lockedPosition;

    private void Start()
    {
        if(_parentScroll == null)
            _parentScroll = GameObject.Find("ScrollRect").GetComponent<ScrollRect>();

        if(_childScroll == null)
            _childScroll = GetComponent<ScrollRect>();
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        _parentScroll.enabled = false;
        _childScroll.enabled = false;

        // 드래그 방향 판단
        _isHorizontalDrag = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);

        // 방향에 따라 ScrollRect 활성화
        _activeScroll = _isHorizontalDrag ? _childScroll : _parentScroll;

        _isDragging = true;


        if (_activeScroll != null)
        {
            _activeScroll.enabled = true;
            _activeScroll.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_activeScroll == null)
            return;

        _activeScroll.OnDrag(eventData);

        // 마지막 위치 계속 저장
        _lockedPosition = _activeScroll.content.anchoredPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        if (_activeScroll != null)
        {
            _activeScroll.OnEndDrag(eventData);
        }
    }

    private void LateUpdate()
    {
        if (!_isDragging || _activeScroll == null)
            return;

        // ScrollRect의 자동 보정 무효화
        _activeScroll.velocity = Vector2.zero;

        // 우리가 저장한 위치로 다시 덮어쓰기
        _activeScroll.content.anchoredPosition = _lockedPosition;
    }
}