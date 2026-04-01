using UnityEngine;
using System.Threading.Tasks;

public class ItemCurrency : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeGold;
    [SerializeField] private IntEventChannelSO _onSellingItem;

    [SerializeField] private int _goldAmount = 1000;

    [SerializeField] private const int _defaultGoldAmount = 0;

    public int GoldAmount
    {
        get
        {
            if (_goldAmount < 0)
                Debug.LogError("현재 Money가 음수입니다.");
            return Mathf.Max(_goldAmount, 0);
        }
    }

    private void Awake()
    {
        _onSellingItem.AddListener(GainGold);  
    }
    private void Start()
    {
        FirebaseManager.Instance.AddLoadData(LoadGold);
        FirebaseManager.Instance.AddSaveData(SaveGold);
    }

    private void OnDestroy()
    {
        _onSellingItem.RemoveListener(GainGold);
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.RemoveLoadData(LoadGold); 
            FirebaseManager.Instance.RemoveSaveData(SaveGold);
        }
    }

    public ItemInstance GetGold()
    {
        var gold = new ItemInstance(ItemID.Gold);
        gold.SetAmount(GoldAmount);
        return gold;
    }

    public void GainGold(int amount)
    {
        _goldAmount += amount;
        _onChangeGold.OnStartEvent();
        _ = SaveGold();
    }

    public void UseGold(int amount)
    {
        if (amount < 0)
            return;

        if(_goldAmount < amount)
        {
            Debug.LogWarning("사용금액이 현재 금액을 초과합니다.");
            return;
        }
        _goldAmount -= amount;
        _onChangeGold.OnStartEvent();
        _ = SaveGold(); 
        
    }
    public bool TryUseGold(int amount)
    {
        if (amount < 0)
            return false;

        if(_goldAmount < amount)
        {
            Debug.LogWarning("사용금액이 현재 금액을 초과합니다.");
            return false;
        }

        return true;
    }

   
    #region Firebase 저장/로드

    public async Task SaveGold()
    {
        await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Gold")
            .SetValueAsync(_goldAmount);
    }

    public async Task LoadGold()
    {
        var snapshot = await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Gold")
            .GetValueAsync();

        _goldAmount = snapshot.Exists
            ? int.Parse(snapshot.Value.ToString())
            : _defaultGoldAmount; // 신규 유저 기본값
    }

    #endregion
}