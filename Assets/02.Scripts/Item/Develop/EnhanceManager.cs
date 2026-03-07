using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceManager : MonoBehaviour
{
    [SerializeField] private Currency _currency;                // 필요 금화를 읽어올 재화 가방
    [SerializeField] private GameObject _inventory;

    private IEnhanceResourceProvider _resourceProvider;
    private PlayerEnhancement _enhanceItem;
    private StackableItem _reinforceResource;
    private StackableItem _gold;
    private bool _isEnhancable;






    //
    [Header("UI관련")]
    public int _itmeID;
    public int _bluePrintID;
    [SerializeField] private Image _enhanceItemImage;
    [SerializeField] private Image _reinforceResourceImage;
    [SerializeField] private Image _goldImage;
    [SerializeField] private TextMeshProUGUI _goldText;
    [SerializeField] private TextMeshProUGUI _costBluePrintText;
    [SerializeField] private TextMeshProUGUI _costGoldText;
    [SerializeField] private TextMeshProUGUI _enhanceLevelText;

    public InventorySlot slot;
    //


    private void Start()
    {
        if (!_inventory.TryGetComponent<IEnhanceResourceProvider>(out var resourceProvider))
        { Debug.LogError("강화재료 제공 인터페이스가 null입니다."); return; }

        _resourceProvider = resourceProvider;
    }

    //
    private void Update()
    {
        _goldText.text = "재화: " + _currency.GoldAmount.ToString();

        if (_enhanceItem != null)
        {
            _enhanceItemImage.sprite = _enhanceItem.SpriteImage;
            _reinforceResourceImage.sprite = _reinforceResource.GetSprite();
            _goldImage.sprite = _gold.GetSprite();
            _costBluePrintText.text = _enhanceItem.EnhanceCostBluePrint.ToString();
            _costGoldText.text = _enhanceItem.EnhanceCostGold.ToString();
            _enhanceLevelText.text = "+" + _enhanceItem.EnhanceLevel.ToString();
        }
        else
        {
            _costBluePrintText.text = "";
            _costGoldText.text = "";
            _enhanceLevelText.text = "";
        }

        if(_resourceProvider != null)
        {
            slot.SetSlot(_bluePrintID, _resourceProvider.ReinforceResourceInventory[_bluePrintID]);
        }
    }
    //

    //
    public void SetEnhancement()
    {
        _enhanceItem = ItemDB.GetItem<PlayerEnhancement>(_itmeID);
        _reinforceResource = ItemDB.GetItem<StackableItem>(_enhanceItem.EnhanceBluePrintID);
        _gold = ItemDB.GetItem<StackableItem>(_currency.MoneyID);
    }
    //


    public void SetEnhancement(int itemID)
    {
        _enhanceItem = ItemDB.GetItem<PlayerEnhancement>(itemID);
        _reinforceResource = ItemDB.GetItem<StackableItem>(_enhanceItem.EnhanceBluePrintID);
        _gold = ItemDB.GetItem<StackableItem>(_currency.MoneyID);
    }


    public void OnEnhance()
    {
        _isEnhancable = true;
        CheckMaxLevel();
        CheckGoldCost();
        CheckResourceCost();

        if (_isEnhancable)
        {
            _enhanceItem.OnEnhance();

            _currency.UseGold(_enhanceItem.EnhanceCostGold);
            var inventory = _resourceProvider.ReinforceResourceInventory;
            _resourceProvider.UseItem(inventory, _reinforceResource.ID, _enhanceItem.EnhanceCostBluePrint);
            Debug.Log("강화성공!");
        }
    }


    // 최대 강화수치인지 확인
    private void CheckMaxLevel()
    {
        if (!_isEnhancable)
            return;

        _isEnhancable = !_enhanceItem.IsEnhanceMaxLevel;

        if (!_isEnhancable)
            Debug.Log("강화수치가 최대입니다.");
    }


    // 골드 재화와 코스트 비교
    private void CheckGoldCost()
    {
        if(!_isEnhancable)
            return;

        int cost = _enhanceItem.EnhanceCostGold;

        if(cost < 0)
        { Debug.LogError("골드 코스트가 음수입니다!"); return; }

        _isEnhancable = _currency.TryUseGold(cost);

        if(!_isEnhancable)
            Debug.Log("골드가 부족합니다.");
    }


    // 재료 재화와 코스트 비교
    private void CheckResourceCost()
    {
        if(!_isEnhancable)
            return;

        int cost = _enhanceItem.EnhanceCostBluePrint;

        if(cost < 0)
        { Debug.LogError("재료 코스트가 음수입니다!"); return; }

        var inventory = _resourceProvider.ReinforceResourceInventory;
        _isEnhancable = _resourceProvider.TryUseItem(inventory, _reinforceResource.ID, cost);

        if (!_isEnhancable)
            Debug.Log("강화 재료가 부족합니다.");
    }
}

public interface IEnhanceResourceProvider
{
    public Dictionary<int, int> ReinforceResourceInventory { get; }

    public void UseItem(Dictionary<int, int> inventory, int id, int amount = 1);
    public bool TryUseItem(Dictionary<int, int> inventory, int id, int amount = 1);
}
