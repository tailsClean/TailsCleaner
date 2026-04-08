using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class ItemInventory : MonoBehaviour
{
    [Header("이벤트 채널")]
    [SerializeField] private VoidEventChannelSO _onChangeInventory;
    [SerializeField] private IntEventChannelSO _onSellingItem;

    // Key: 아이템 , Value: 소지갯수
    private Dictionary<ItemInstance, int> _inventory;

    public Dictionary<ItemInstance, int> Inventory => _inventory;

    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;


    [ContextMenu("인벤토리 초기화")]
    public void ClearInventory()
    {
        _inventory.Clear();
        _onChangeInventory.OnStartEvent();
    }

    private void Awake()
    {
        _inventory = new Dictionary<ItemInstance, int>();
    }
    private void Start()
    {
        Debug.Log($"ItemInventory Start - FirebaseManager: {FirebaseManager.Instance}");
        FirebaseManager.Instance.AddLoadData(LoadInventory);
        FirebaseManager.Instance.AddSaveData(SaveInventory);
        Debug.Log("SaveInventory 등록 완료");
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            
            FirebaseManager.Instance.RemoveLoadData(LoadInventory);
            FirebaseManager.Instance.RemoveSaveData(SaveInventory);
        }
    }


    public void InitEvent()
    {
        OnAddItem = null;
        OnRemoveItem = null;
    }

    public void SellItem(ItemInstance item, int amount = 1)
    {
        bool isSuccess = false;
        if(item.ItemType == ITEM_TYPE.Equipment)
            isSuccess = RemoveEquipment(item, amount);


        if (!isSuccess)
        { Debug.Log($"<color=red>아이템 판매에 실패했습니다.</color> \n판매하려는 아이템: {item.Name}({item.ID})"); return; }


        if (!ItemDB.TryGetData<MaterialEquipData>(item.ID, out var material))
            return;

        _onSellingItem.OnStartEvent(material.EquipMatter.price * amount);
    }


    #region 장비 아이템


    // 장비 아이템 읽기
    public bool TryGetEquipment(int id, GRADE grade, out ItemInstance item) => TryGetItem(id, ItemInstance.NoneEnhanceLevel, grade, out item);
    public ItemInstance GetEquipment(int id, int enhanceLevel, GRADE grade) => GetItem(id, enhanceLevel, grade);


    // 장비 아이템 획득
    public void GainEquipment(int id, int amount = 1) => 
        GainItem(id, ItemInstance.NoneEnhanceLevel, (GRADE)(id % 100 - 1), amount);


    /// 장비 아이템 제거
    public bool RemoveEquipment(int id, int amount = 1) =>
        UseItem(id, ItemInstance.NoneEnhanceLevel, (GRADE)(id % 100 - 1), amount);


    public bool RemoveEquipment(ItemInstance item, int amount = 1) => 
        UseItem(item.ID, item.EnhanceLevel, item.Grade, amount);



    #endregion


    #region 유물 아이템


    public void AddRelic(int id)
    {
        // 이미 가지고 있는지 확인 
        if (HasRelic(id))
        {
            Debug.LogWarning($"[Relic] ID:{id} 유물은 이미 소지 중이라 추가하지 않습니다.");
            return;
        }

        // 딕셔너리에 추가
        _inventory.Add(new ItemInstance(id, 0, GRADE.None), 1);

        // 데이터가 변했으니 이벤트 발생 및 저장
        _onChangeInventory?.OnStartEvent();
        _ = SaveInventory(); // Firebase 저장 (비동기 호출)

        Debug.Log($"[Relic] ID:{id} 유물 인벤토리에 추가 완료!");
    }


    // 유물 아이템 소지 여부 확인
    public bool HasRelic(int id)
    {
        var item = SearchItem(id, 0, GRADE.None);
        return HasItem(item);
    }


    /// 유물 아이템 읽기
    public bool TryGetRelic(int id, int enhanceLevel, out ItemInstance item) =>
        TryGetItem(id, enhanceLevel, GRADE.None, out item);
    public ItemInstance GetRelic(int id, int enhanceLevel) => GetItem(id, enhanceLevel, GRADE.None);


    // 유물 아이템 획득
    public void GainRelic(int id, int enhanceLevel)
    {
        ItemInstance item = default;
        for(int i = 0; i < 10; i++)
        {
            item = SearchItem(id, i, GRADE.None);

            if (!HasItem(item))
                continue;

            else
            {
                Debug.LogWarning($"{id} 유물은 더이상 획득할 수 없습니다.");
                break;
            }
        }

        if(!HasItem(item))
            _inventory.Add(new ItemInstance(id, enhanceLevel, GRADE.None), 1);

        //// 아이템을 소지하고 있었을 때
        //if (HasItem(item))


            // 아이템을 소지하지 않았을 때
        //else
        //{
        //    _inventory.Add(new ItemInstance(id, enhanceLevel, GRADE.None), 1);
        //}

        _onChangeInventory.OnStartEvent();
        _ = SaveInventory();
    }

     public void GainRelicNoSave(int id, int enhanceLevel)
    {
        ItemInstance item = default;
        for(int i = 0; i < 10; i++)
        {
            item = SearchItem(id, i, GRADE.None);

            if (!HasItem(item))
                continue;

            else
            {
                Debug.LogWarning($"{id} 유물은 더이상 획득할 수 없습니다.");
                break;
            }
        }

        if(!HasItem(item))
            _inventory.Add(new ItemInstance(id, enhanceLevel, GRADE.None), 1);

        //// 아이템을 소지하고 있었을 때
        //if (HasItem(item))


            // 아이템을 소지하지 않았을 때
        //else
        //{
        //    _inventory.Add(new ItemInstance(id, enhanceLevel, GRADE.None), 1);
        //}

        _onChangeInventory.OnStartEvent();
        
    }


    // 유물 아이템 삭제
    public bool RemoveRelic(int id, int enhanceLevel)
    {
        var item = SearchItem(id, enhanceLevel, GRADE.None);

        if (!HasItem(item))
        { Debug.Log($"사용하려는 {id} 아이템을 소지하지 않았습니다."); return false; }

        _inventory.Remove(item);
        _onChangeInventory.OnStartEvent();
        _ = SaveInventory();

        return true;
        
    }


    #endregion


    #region 스택형 아이템(소모품, 강화 재료)


    // 스택형 아이템 읽기
    public bool TryGetStackItem(int id, out ItemInstance item) => TryGetItem(id, ItemInstance.NoneEnhanceLevel, GRADE.None, out item);
    public ItemInstance GetStackItem(int id) => GetItem(id, ItemInstance.NoneEnhanceLevel, GRADE.None);


    // 스택형 아이템 획득
    public void GainStackItem(int id, int amount = 1)
    {
        GainItem(id, ItemInstance.NoneEnhanceLevel, GRADE.None, amount);
    }
    public void GainStackItemNoSave(int id, int amount = 1)
    {
        GainItemNoSave(id, ItemInstance.NoneEnhanceLevel, GRADE.None, amount);
    }

    // 스택형 아이템 사용
    public bool UseStackItem(int id, int amount) => 
        UseItem(id, ItemInstance.NoneEnhanceLevel, GRADE.None, amount);


    #endregion



    #region 인벤토리 내부전용 메서드

    
    private bool TryGetItem(int id, int enhanceLevel, GRADE grad, out ItemInstance item)
    {
        item = SearchItem(id, enhanceLevel, grad);
        if (HasItem(item))
        {
            item.SetAmount(_inventory[item]);
            return true;
        }

        Debug.LogWarning($"{id} 아이템은 인벤토리에 없습니다.");
        return false;
    }

    // 인벤토리 내부전용 아이템 확인 메서드
    private ItemInstance GetItem(int id, int enhanceLevel, GRADE grad)
    {
        var item = SearchItem(id, enhanceLevel, grad);
        if (HasItem(item))
        {
            item.SetAmount(_inventory[item]);
            return item;
        }

        Debug.LogError($"{id} / 강화{enhanceLevel}아이템은 인벤토리에 없습니다.");
        return item;
    }


    // 인벤토리 내부전용 아이템 획득 메서드
    private void GainItem(int id, int enhanceLevel, GRADE grad, int amount)
    {
        var item = SearchItem(id, enhanceLevel, grad);

        if (HasItem(item))
            _inventory[item] += amount;

        else
            _inventory.Add(new ItemInstance(id, enhanceLevel, grad), amount);

        _onChangeInventory.OnStartEvent();
        _ = SaveInventory();
    }

    private void GainItemNoSave(int id, int enhanceLevel, GRADE grad, int amount)
    {
        var item = SearchItem(id, enhanceLevel, grad);

        if (HasItem(item))
            _inventory[item] += amount;

        else
            _inventory.Add(new ItemInstance(id, enhanceLevel, grad), amount);

        _onChangeInventory.OnStartEvent();
        
    }

    // 인벤토리 내부전용 아이템 사용 메서드
    private bool UseItem(int id, int enhanceLevel, GRADE grade, int amount)
    {
        if (amount < 0)
        { Debug.LogError("사용하려는 값이 음수입니다."); return false; }

        var item = SearchItem(id, enhanceLevel, grade);
        if (!HasItem(item))
        { Debug.Log($"사용하려는 {id} 아이템을 소지하지 않았습니다."); return false; }

        if (_inventory[item] < amount)
        { Debug.Log($"사용량이 {id} 아이템 소지갯수를 초과합니다."); return false; }

        _inventory[item] -= amount;

        if (_inventory[item] == 0)
            _inventory.Remove(item);
        _onChangeInventory.OnStartEvent();
        _ = SaveInventory();
        return true;
    }


    // 인벤토리 내부전용 아이템 찾기 메서드
    private ItemInstance SearchItem(int id, int enhanceLevel, GRADE grade)
    {
        foreach (var item in _inventory)
        {
            bool isItem = item.Key.ID == id &&
                          item.Key.EnhanceLevel == enhanceLevel &&
                          item.Key.Grade == grade;
            if (isItem)
                return item.Key;
        }
        return ItemInstance.None;
    }

    // 인벤토리 내부전용 아이템 사용 메서드
    private bool HasItem(ItemInstance invenItem)
    {
        return !(invenItem.ID == ItemInstance.None.ID &&
       invenItem.EnhanceLevel == ItemInstance.None.EnhanceLevel &&
       invenItem.Grade == ItemInstance.None.Grade);
    }


    #endregion

    #region  firebase 저장/로드

    private bool _isLoading = false;

    public async Task SaveInventory()
    {
        if (_isLoading) return;
        
        var inventoryData = new Dictionary<string, object>
        {
            { "Equipments", new Dictionary<string, object>() },
            { "Relics", new Dictionary<string, object>() },
            { "Stacks", new Dictionary<string, object>() }
        };

        int equipIndex = 0;
        int relicIndex = 0;
        int stackIndex = 0;

        foreach (var kvp in _inventory)
        {
            var item = kvp.Key;
            var amount = kvp.Value;

            switch (item.ItemType)
            {
                case ITEM_TYPE.Equipment:
                    ((Dictionary<string, object>)inventoryData["Equipments"])[$"{equipIndex}"] = new Dictionary<string, object>
                    {
                        { "id", item.ID },
                        { "enhanceLevel", item.EnhanceLevel },
                        { "grade", (int)item.Grade },
                        { "amount", amount }
                    };
                    equipIndex++;
                    break;

                case ITEM_TYPE.Relic:
                    ((Dictionary<string, object>)inventoryData["Relics"])[$"{relicIndex}"] = new Dictionary<string, object>
                    {
                        { "id", item.ID },
                        { "enhanceLevel", item.EnhanceLevel }
                    };
                    relicIndex++;
                    break;

                default:
                    ((Dictionary<string, object>)inventoryData["Stacks"])[$"{stackIndex}"] = new Dictionary<string, object>
                    {
                        { "id", item.ID },
                        { "amount", amount }
                    };
                    stackIndex++;
                    break;
        }
    }

    await FirebaseManager.Instance.DB
        .Child("users")
        .Child(FirebaseManager.Instance.UID)
        .Child("Inventory")
        .SetValueAsync(inventoryData); 
        
    }

    public async Task LoadInventory()
    {
        _isLoading = true;
        var snapshot = await FirebaseManager.Instance.DB
            .Child("users")
            .Child(FirebaseManager.Instance.UID)
            .Child("Inventory")
            .GetValueAsync();

        if (!snapshot.Exists) 
        {
            _isLoading = false;
            return;
        }

        // 장비 로드
        var equipSnapshot = snapshot.Child("Equipments");
        foreach (var child in equipSnapshot.Children)
        {
            int id = int.Parse(child.Child("id").Value.ToString());
            int enhanceLevel = int.Parse(child.Child("enhanceLevel").Value.ToString());
            GRADE grade = (GRADE)int.Parse(child.Child("grade").Value.ToString());
            int amount = int.Parse(child.Child("amount").Value.ToString());

            GainItemNoSave(id, enhanceLevel, grade, amount);
        }

        // 유물 로드
        var relicSnapshot = snapshot.Child("Relics");
        foreach (var child in relicSnapshot.Children)
        {
            int id = int.Parse(child.Child("id").Value.ToString());
            int enhanceLevel = int.Parse(child.Child("enhanceLevel").Value.ToString());

            GainRelicNoSave(id, enhanceLevel);
        }

        // 스택형 아이템 로드
        var stackSnapshot = snapshot.Child("Stacks");
        foreach (var child in stackSnapshot.Children)
        {
            int id = int.Parse(child.Child("id").Value.ToString());
            int amount = int.Parse(child.Child("amount").Value.ToString());

            GainStackItemNoSave(id, amount);
        }

        _onChangeInventory.OnStartEvent();
        _isLoading = false;
    }

#endregion
}
