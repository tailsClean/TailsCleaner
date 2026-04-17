using UnityEngine;
using UnityEngine.UI;
using static UISlotAddedText;


public class RelicLoadoutPopup : ItemPopupBase
{
    [Header("=============== 착용 유물 팝업 전용 ==================================")]
    [Header("판매용 버튼")]
    [SerializeField] private Button _releaseButton;

    private PlayerLoadout _playerLoadout;


    protected override void Start()
    {
        base.Start();
        _playerLoadout = PlayerStatManager.Instance.Loadout;
        SetReleaseButton();
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
        if (!ItemDB.TryGetData<RelicData>(_currentItem.ID, out var itemData))
        { Debug.LogWarning($"{_currentItem.Name}({_currentItem.ID})의 정보를 읽어올 수 없습니다.", this); return; }

        string nameText = itemData.Name;
        string descText = GetItemDesc(itemData);

        _itemSlot.SetAddedText(TEXT_TYPE.Name, nameText);
        _itemSlot.SetAddedText(TEXT_TYPE.Desc, descText);
    }

    // 아이템의 설명
    private string GetItemDesc(RelicData relic)
    {

        string itemScrptKey = relic.Relic.script;
        StringSO stringDataSO = DataManager.Instance.GetSOData<StringSO>();
        var stringData = stringDataSO.GetById(itemScrptKey);

        return stringData != null ? stringData.kr : string.Empty;
    }


    // 장착 해제 기능 버튼에 추가
    private void SetReleaseButton()
    {
        if (_playerLoadout == null)
            return;

        if (_releaseButton == null)
        { Debug.LogWarning("장착 해제 버튼이 없습니다.", this); return; }

        _releaseButton.onClick.AddListener( () => gameObject.SetActive(false) );
        _releaseButton.onClick.AddListener( () => _playerLoadout.RemoveRelic(_currentItem.ID, _currentItem.EnhanceLevel) );
    }


    #endregion
}