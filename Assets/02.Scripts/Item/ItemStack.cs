
/// <summary>
/// 인벤토리의 아이템의 정보와 수량을 담은 정보 전달용 구조체
/// </summary>
public struct ItemStack
{
    public readonly ItemBaseSO ItemData;
    public readonly int Amount;

    public ItemStack(int id, int amount)
    {
        ItemData = ItemDB.GetItemData<ItemBaseSO>(id);
        Amount = amount;
    }

    public ItemStack(ItemBaseSO itemData, int amount)
    {
        ItemData = itemData;
        Amount = amount;
    }
}

