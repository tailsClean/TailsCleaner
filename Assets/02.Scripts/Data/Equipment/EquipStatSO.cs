
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipStatSO", menuName = "Data/EquipStatSO")]
public class EquipStatSO : ScriptableObject
{
    public List<EquipStat> dataList = new List<EquipStat>();

    public EquipStat GetById(int id)
        => dataList.Find(x => x.id == id);
}
