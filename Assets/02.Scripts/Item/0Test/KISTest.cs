using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
public class KISTest : MonoBehaviour, IEnhanceResourceProvider
{
    public Dictionary<int, int> ReinforceResourceInventory { get; private set; }

    public int Id;
    public int Amount;

    private void Awake()
    {
        ReinforceResourceInventory = new Dictionary<int, int>();

        ReinforceResourceInventory.Add(Id, Amount);
    }

    [ContextMenu("재충전")]
    private void Set()
    {
        ReinforceResourceInventory[Id] = Amount;
    }

    public bool TryUseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (ReinforceResourceInventory[Id] <  amount)
            return false;


        return true;
    }

    public void UseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        ReinforceResourceInventory[Id] -= amount;
    }
}
#endif
