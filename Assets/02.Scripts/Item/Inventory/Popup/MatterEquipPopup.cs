using UnityEngine;
using UnityEngine.UI;
using static UISlotAddedText;


public class MatterEquipPopup : ItemPopupBase
{
    [Header("=============== 재료장비 팝업 전용 ==================================")]
    [Header("판매용 버튼")]
    [SerializeField] private Button _sellingButton;
    [SerializeField] private SellingItemPopup _sellingPopup;



    protected override void Start()
    {
        base.Start();
        SetSellingButton();
    }

    public override void SetSlot(ItemInstance itemInstance)
    {
        base.SetSlot(itemInstance);
        SlotNameAndDesc();
    }


    #region 내부 메서드

    // 아이템 이름과 설명 출력
    private void SlotNameAndDesc()
    {
        if (!ItemDB.TryGetData<MaterialEquipData>(_currentItem.ID, out var itemData))
        { Debug.LogWarning($"{_currentItem.Name}({_currentItem.ID})의 정보를 읽어올 수 없습니다.", this); return; }

        string nameText = itemData.Name;
        string descText = GetItemDesc(itemData);

        _itemSlot.SetAddedText(TEXT_TYPE.Name, nameText);
        _itemSlot.SetAddedText(TEXT_TYPE.Desc, descText);
    }

    // 아이템의 설명
    private string GetItemDesc(MaterialEquipData equip)
    {

        string itemScrptKey = equip.EquipMatter.script;
        StringSO stringDataSO = DataManager.Instance.GetSOData<StringSO>();
        var stringData = stringDataSO.GetById(itemScrptKey);

        return stringData != null ? stringData.kr : string.Empty;
    }


    // 판매 기능 버튼에 추가
    private void SetSellingButton()
    {
        if(_sellingButton != null && _sellingPopup != null)
        {
            _sellingButton.onClick.AddListener(() => gameObject.SetActive(false));
            _sellingButton.onClick.AddListener(() => _sellingPopup.gameObject.SetActive(true));
            _sellingButton.onClick.AddListener(() => _sellingPopup.SetSlot(_currentItem));
        }
    }

    #endregion
}