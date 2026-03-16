
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipEnhanceSO", menuName = "Data/EquipEnhanceSO")]
public class EquipEnhanceSO : ScriptableObject
{
    public List<EquipEnhance> dataList = new List<EquipEnhance>();

    public EquipEnhance GetById(int id)
        => dataList.Find(x => x.id == id);
}
