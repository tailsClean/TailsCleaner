
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternGroupCompositionSO", menuName = "Data/PatternGroupCompositionSO")]
public class PatternGroupCompositionSO : ScriptableObject
{
    public List<PatternGroupComposition> dataList = new List<PatternGroupComposition>();

    public PatternGroupComposition GetById(int id)
        => dataList.Find(x => x.pattern_group_id == id);

    // 추가
    public List<PatternGroupComposition> GetAllByGroupId(int groupId)
        => dataList
            .Where(x => x.pattern_group_id == groupId)
            .OrderBy(x => x.priority)
            .ToList();
}
