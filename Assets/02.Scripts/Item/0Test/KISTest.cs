using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class KISTest : MonoBehaviour
{
    public Inventory inventory;
    public int Id;
    public int Amount;



    [ContextMenu("재료 재충전")]
    private void Set()
    {
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.WeaponReinforceResource, Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.HatReinforceResource   , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.CloakReinforceResource , Amount);
        inventory.GainItem(ITEM_TYPE.Reinforcement, ItemID.ShoseReinforceResource , Amount);
    }
}
#endif
