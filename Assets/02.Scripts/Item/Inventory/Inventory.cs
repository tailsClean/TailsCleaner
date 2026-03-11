using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Inventory : MonoBehaviour, IEnhanceResourceProvider
{
    // Key: 아이템ID , Value: 소지갯수
    private Dictionary<int, int> _equipInventory;
    private Dictionary<int, int> _relicInventory;
    private Dictionary<int, int> _reinforceResourceInventory;
    private Dictionary<int, int> _spendableInventory;


    public Dictionary<int, int> EquipInventory => _equipInventory;
    public Dictionary<int, int> RelicInventory => _relicInventory;
    public Dictionary<int, int> ReinforceResourceInventory => _reinforceResourceInventory;
    public Dictionary<int, int> SpendableInventory => _spendableInventory;


    public event Action<int> OnAddItem;
    public event Action<int> OnRemoveItem;

    public void InitEvent()
    {
        OnAddItem = null;
        OnRemoveItem = null;
    }


    private void Awake()
    {
        _equipInventory = new Dictionary<int, int>();
        _relicInventory = new Dictionary<int, int>();
        _reinforceResourceInventory = new Dictionary<int, int>();
        _spendableInventory = new Dictionary<int, int>();
    }








    //
    private Dictionary<int, int> test;
    public void TestUIGroup(int i)
    {
        switch (i)
        {
            case 0: test = _equipInventory; break;
            case 1: test = _relicInventory; break;
            case 2: test = _reinforceResourceInventory; break;
            case 3: test = _spendableInventory; break;
        }
    }

    public void TestGain(int id)
    {
        Debug.Log(test);
        GainItem(test, id);
    }
    public void TestUse( int id) => TryUseItem(test, id);
    //



    // 아이템 획득시, 인벤토리 저장
    public void GainItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (inventory.TryGetValue(id, out var item))
            inventory[id] += amount;

        else
        {
            inventory.Add(id, amount);
            OnAddItem?.Invoke(id);
        }
    }

    // 인벤토리의 아이템 사용
    public void UseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if (!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        { Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>"); return; }

        if (itemCount > amount)
            inventory[id] -= amount;

        else if (itemCount == amount)
        {
            inventory.Remove(id);
            OnRemoveItem?.Invoke(id);
        }

        else
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
    }


    // 아이템 사용가능 여부
    public bool TryUseItem(Dictionary<int, int> inventory, int id, int amount = 1)
    {
        if(!inventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return false;
        }

        if(itemCount < amount)
        {
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
            return false;
        }

        return true;
    }

}