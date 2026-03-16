
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternGroupSO", menuName = "Data/PatternGroupSO")]
public class PatternGroupSO : ScriptableObject
{
    public List<PatternGroup> dataList = new List<PatternGroup>();

    public PatternGroup GetById(int id)
        => dataList.Find(x => x.pattern_group_id == id);
}
