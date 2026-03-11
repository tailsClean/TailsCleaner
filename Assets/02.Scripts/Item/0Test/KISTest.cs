using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class KISTest : MonoBehaviour
{
    public Inventory inventory;
    public int Id;
    public int Amount;



    [ContextMenu("강화 재료 재충전")]
    private void SetItem()
    {
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.WeaponReinforceResource, Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.HatReinforceResource   , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.CloakReinforceResource , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.ShoseReinforceResource , Amount);
    }

    [ContextMenu("유물 획득")]
    private void Set()
    {
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.RelicReinforceResource, Amount);
        inventory.SetRelic(new RelicStatus(0, 50, 0));
        inventory.SetRelic(new RelicStatus(1, 60, 0));
        inventory.SetRelic(new RelicStatus(2, 70, 0));

    }


}
#endif
