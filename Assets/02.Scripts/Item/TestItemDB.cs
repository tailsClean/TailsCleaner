using UnityEngine;

public class TestItemDB : MonoBehaviour
{
    private void Start()
    {
        var a = ItemDB.GetItem<EquipmentBase>(100);
        Instantiate(a);

        var b = ItemDB.GetItem<EquipmentBase>(1000);
        Instantiate(b);

        var c = ItemDB.GetItem<RelicBase>(10000);
        Instantiate(c);
    }

}
