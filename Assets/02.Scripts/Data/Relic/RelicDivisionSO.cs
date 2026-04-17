using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicDivisionSO", menuName = "Data/RelicDivisionSO")]
public class RelicDivisionSO : ScriptableObject
{
    public List<RelicDivision> dataList = new List<RelicDivision>();

    public RelicDivision GetById(int id)
        => dataList.Find(x => x.id == id);
}
