using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public abstract class ItemPopupBase : MonoBehaviour
{
    [Header("=============== 팝업UI 기본 필드 ==================================")]
    [SerializeField] protected Button _background;                          // 백그라운드 클릭시 나가기
    [SerializeField] protected UISlotAddedText _itemSlot;
    [SerializeField] protected List<InventoryUIOpenSet> _openButton;        // 특정 버튼으로 지정된 UIGroup으로 이동
    [field: SerializeField] public ITEM_TYPE ItemType { get; private set; }

    protected ItemInstance _currentItem;



    protected virtual void Start()
    {
        SetButton();
    }


    // 팝업이 열릴 때, 해당 아이템 세팅 및 버튼에 메서드 추가
    public virtual void SetSlot(ItemInstance itemInstance)
    {
        _currentItem = itemInstance;
        _itemSlot.SetSlot(itemInstance.ID);
    }

    // 버튼 초기화
    private void SetButton()
    {
        // 백그라운드 클릭시, 팝업 닫히도록 메서드 추가
        var parent = _background.transform.parent;
        _background.onClick.AddListener( () => parent.gameObject.SetActive(false));

        // 버튼을 누르면 특정 UIGroup만 열리고 다른 창은 닫는 기능 버튼에 추가
        foreach(var button in _openButton)
        {
            button.PushButton.onClick.AddListener(() => OpenUI(button.OpenGroup));
        }
    }
    // 아이템 매니저에서 UIGroup 전체 닫히는 메서드 불러오기
    private void OpenUI(UI_GROUP group) => ItemManager.Instance.OpenUI(group);

}