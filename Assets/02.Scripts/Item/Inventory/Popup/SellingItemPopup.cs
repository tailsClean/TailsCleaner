using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SellingItemPopup : ItemPopupBase
{
    [Header("=============== 판매 팝업 전용 ==================================")]
    [SerializeField] ItemInventory _inventory;

    [Header("갯수 관련")]
    [SerializeField] private List<Button> _upDownButton;
    [SerializeField] private List<TextMeshProUGUI> _countText;      // 0: 10의 자리 , 1: 1의 자리

    [Header("아랫줄 가격과 취소, 판매 버튼")]
    [SerializeField] private Button _cancelSellButton;
    [SerializeField] private Button _executeSellButton;
    [SerializeField] private TextMeshProUGUI _priceText;

    [Header("취소 판매 버튼 관련")]
    [SerializeField] private MatterEquipPopup _equipStatusPopup;
    [SerializeField] private Image _sellingPopupImg;



    private int _count;
    private int _price;
    private bool _isbinding = false;

    public int TenCount => _count / 10;
    public int OneCount => _count % 10;

    protected override void Start()
    {
        // 부모 클래스의 스타트 메서드 실행을 하지 않기 위함
    }    


    public override void SetSlot(ItemInstance item)
    {
        base.SetSlot(item);
        SetButton();
        SetCount(-_count);
    }


    private void SetButton()
    {
        if (_isbinding)
            return;

        _upDownButton[0].onClick.AddListener(() => SetCount(10));
        _upDownButton[1].onClick.AddListener(() => SetCount(-10));
        _upDownButton[2].onClick.AddListener(() => SetCount(1));
        _upDownButton[3].onClick.AddListener(() => SetCount(-1));

        _cancelSellButton.onClick.AddListener(Cancle);
        _executeSellButton.onClick.AddListener(Selling);

        _isbinding = true;
    }

    private void Cancle()
    {
        gameObject.SetActive(false);
        _equipStatusPopup.gameObject.SetActive(true);
    }

    private void Selling()
    {
        if(_sellingPopupImg != null)
        {
            _sellingPopupImg.gameObject.SetActive(true);
            StartCoroutine(ReleaseSellingPopup());
        }

        Debug.Log($"{_price}의 가격으로 판매 완료");
        _inventory.SellItem(_currentItem, _count);
        gameObject.SetActive(false);
    }


    private IEnumerator ReleaseSellingPopup()
    {
        yield return new WaitForSeconds(1f);
        _sellingPopupImg.gameObject.SetActive(false);
    }

    private void SetCount(int count)
    {
        _count += count;

        if (_count <= 0)
            _count = 0;

        if(_count >= _currentItem.Amount)
            _count = _currentItem.Amount;

        _countText[0].text = TenCount.ToString();
        _countText[1].text = OneCount.ToString();

        int price = 0;
        if (ItemDB.TryGetData<MaterialEquipData>(_currentItem.ID, out var materialEquip))
            price = materialEquip.EquipMatter.price;

        _price = price * _count;
        _priceText.text = _price.ToString();

    }
}