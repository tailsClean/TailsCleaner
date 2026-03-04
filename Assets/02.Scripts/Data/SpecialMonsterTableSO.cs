// 자동 생성 됨.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpecialMonsterTableSO", menuName = "Data/SpecialMonsterTableSO")]
public class SpecialMonsterTableSO : ScriptableObject
{
    public List<SpecialMonsterTable> dataList = new List<SpecialMonsterTable>();

    public SpecialMonsterTable GetById(int id)
        => dataList.Find(x => x.special_id == id);
}
