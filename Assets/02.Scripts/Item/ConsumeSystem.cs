using UnityEngine;


public interface IConsumItemTarget
{
    public void IncreaseValue(float value);
    public bool IsMaxValue { get; }
}

public class ConsumeSystem
{
    private EnergySystem _energySystem => GameManager.Instance._energySystem;             // 에너지 회복용도
    private OutGameLevelSystem _levelSystem;        // 게임레벨(경험치) 증가 용도
    private ItemInventory _inventory;

    public ConsumeSystem(ItemInventory inventory)
    {
        _levelSystem = OutGameLevelSystem.Instance;
        _inventory = inventory;
    }

    public void UseItem(ItemInstance item, int amount, out bool isConsume)
    {
        isConsume = false;

        // DB접근없이 소비탬이 아니면 사용 제한
        if (item.ItemType != ITEM_TYPE.Consume)
        { Debug.LogWarning("소비탬이 아닌 것을 소모하려 했습니다."); return; }

        // DB접근으로 아이템 확인
        if (!ItemDB.TryGetData<ItemManageData>(item.ID, out var consumItem))
            return;

        if (consumItem.Consume == null)
            return;

        var inventoryItem = _inventory.GetStackItem(item.ID);
        if (inventoryItem.Amount < amount)
        { WarningText.ShowText("인벤토리의 아이템 갯수가 부족합니다."); return; }

        for(int i = 0; i < amount; i++)
        {
            UseItem(consumItem, item);
        }

        isConsume = true;
    }

    // 소비탬의 용법에 맞춰서 소비처 나누기
    private void UseItem(ItemManageData consumItem, ItemInstance item)
    {
        var consum = consumItem.Consume;
        bool isUseable = false;
        switch (consum.item_stat_type)
        {
            // 에너지 증가
            case ITEM_CONSUME_TYPE.EnergyUp:
                isUseable = CheckConsumable(_energySystem, consum.item_opt);
                break;

            // 아웃게임 경험치 증가
            case ITEM_CONSUME_TYPE.Exp:
                isUseable = CheckConsumable(_levelSystem, consum.item_opt);
                break;

            default:
                return;
        }

        if(isUseable)
        {
            _inventory.UseStackItem(item.ID, 1);
            UsedText(consumItem);
        }
    }

    private bool CheckConsumable(IConsumItemTarget consumItemTarget, float value)
    {
        if (consumItemTarget.IsMaxValue)
        { WarningText.ShowText("최대치 이상으로 회복할 수 없습니다."); return false; }

        consumItemTarget.IncreaseValue(value);
        return true;
    }

    private void UsedText(ItemManageData consumItem)
    {
        var consum = consumItem.Consume;
        switch (consum.item_stat_type)
        {
            // 에너지 증가
            case ITEM_CONSUME_TYPE.EnergyUp:
                WarningText.ShowText("<color=green>에너지 아이템을 사용했습니다.</color>");
                break;

            // 아웃게임 경험치 증가
            case ITEM_CONSUME_TYPE.Exp:
                WarningText.ShowText("<color=green>경험치 아이템을 사용했습니다.</color>");
                break;

            default:
                return;
        }
    }
}
