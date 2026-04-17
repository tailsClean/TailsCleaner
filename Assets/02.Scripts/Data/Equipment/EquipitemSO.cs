
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipitemSO", menuName = "Data/EquipitemSO")]
public class EquipitemSO : ScriptableObject
{
    public List<Equipitem> dataList = new List<Equipitem>();

    public Equipitem GetById(int id)
        => dataList.Find(x => x.id == id);
}
