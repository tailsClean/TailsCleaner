
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipGradeSO", menuName = "Data/EquipGradeSO")]
public class EquipGradeSO : ScriptableObject
{
    public List<EquipGrade> dataList = new List<EquipGrade>();

    public EquipGrade GetById(int id)
        => dataList.Find(x => x.id == id);
}
