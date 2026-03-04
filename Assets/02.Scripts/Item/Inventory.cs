using System;
using System.Collections.Generic;
using UnityEngine;


public class Inventory : MonoBehaviour
{
    [SerializeField] private Dictionary<int, int> _myInventory;         // Key: 아이템ID , Value: 소지갯수

    public event Action<int> OnRemoveItem;

    private void Awake()
    {
        _myInventory = new Dictionary<int, int>();

        //
        for(int i = 0; i < 15; i++)
        {
            var a = Instantiate(Icon, transform);
            Icons.Add(a);
        }
        //
    }

    public void TestGain(int id)
    {
        GainItem(id);
        foreach(var a in _myInventory)
        {
            Debug.Log("<color=green>인벤: " + a.Key + "</color>");
        }
    }
    public void TestUse(int id) => UseItem(id);

    // 아이템 획득시, 인벤토리 저장
    public void GainItem(int id, int amount = 1)
    {
        if (_myInventory.TryGetValue(id, out var item))
            _myInventory[id] += amount;

        else
            _myInventory.Add(id, amount);
    }

    // 인벤토리의 아이템 사용
    public void UseItem(int id, int amount = 1)
    {
        if(!_myInventory.TryGetValue(id, out var itemCount) || itemCount <= 0)
        {
            Debug.Log($"<color=red>ID: {id}의 아이템을 가지고 있지 않습니다.</color>");
            return;
        }

        if(itemCount > amount)
            _myInventory[id] -= amount;

        else if(itemCount == amount)
        {
            _myInventory.Remove(id);
            OnRemoveItem?.Invoke(id);
        }

        else if(itemCount < amount)
            Debug.Log($"ID: {id}의 아이템의 소지갯수가 부족합니다.");
    }





    //
    public TestItemIcon Icon;
    public List<TestItemIcon> Icons;

    private Dictionary<int, int> IconDict = new();

    public Sprite baseSprite;

    private void Start()
    {
        baseSprite = Icon.baseSprite;
        OnRemoveItem += RemoveIcon;
    }

    private void Update()
    {
        int index = 0;
        foreach (var itemID in _myInventory)
        {
            IconDict.TryAdd(itemID.Key, index);
            Icons[index++].SetIcon(itemID.Key, itemID.Value);
        }
    }

    public void RemoveIcon(int id)
    {
        int index = IconDict[id];
        var icon = Icons[index];
        icon.Init();
        icon.transform.SetAsLastSibling();

        IconDict.Remove(id);
        Icons.Remove(icon);
        Icons.Add(icon);
    }
    //
}