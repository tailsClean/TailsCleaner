

using UnityEngine;

/// <summary>
/// 인벤토리의 아이템의 정보와 수량을 담은 정보 전달용 구조체
/// </summary>
public struct ItemStack
{
    public readonly ItemBaseSO ItemData;
    public int InstanceID;
    public readonly int EnhanceLevel;
    public readonly int Amount;

    public ItemStack(int id, int amount, int instanceID = 0)
    {
        ItemData = ItemDB.GetItemData<ItemBaseSO>(id);
        InstanceID = instanceID;
        Amount = amount;
        EnhanceLevel = default;
    }

    public ItemStack(ItemBaseSO itemData, int amount, int instanceID = 0)
    {
        ItemData = itemData;
        InstanceID = instanceID;
        Amount = amount;
        EnhanceLevel = default;
    }

    public ItemStack(int id, int amount, int enhanceLevel, int instanceID = 0)
    {
        ItemData = ItemDB.GetItemData<ItemBaseSO>(id);
        InstanceID = instanceID;
        Amount = amount;
        EnhanceLevel = enhanceLevel;
    }
}

