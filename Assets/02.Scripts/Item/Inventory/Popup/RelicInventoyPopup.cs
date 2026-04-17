
using UnityEngine;
using UnityEngine.UI;
using static UISlotAddedText;


public class RelicInventoyPopup : ItemPopupBase
{
    [Header("=============== 유물 팝업 전용 ==================================")]
    [Header("장착 버튼")]
    [SerializeField] private Button _equipedButton;

    private PlayerLoadout _playerLoadout;


    protected override void Start()
    {
        base.Start();
        _playerLoadout = PlayerStatManager.Instance.Loadout;
    }

    public override void SetSlot(ItemInstance item)
    {
        base.SetSlot(item);
        SlotNameAndDesc();
        SetEquipedButton();
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

    // 장착 버튼 세팅
    private void SetEquipedButton()
    {
        _equipedButton.onClick.RemoveAllListeners();
        _equipedButton.onClick.AddListener(SetRelic);
        _equipedButton.onClick.AddListener(EquipedNext);
    }

    // 장비창에 유물 장착 메서드
    private void SetRelic()
    {
        _playerLoadout.SetRelic(_currentItem);
    }

    // 장착 후에 호출하는 메서드
    private void EquipedNext() => transform.gameObject.SetActive(false);

    #endregion
}