// 자동 생성 됨.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternSO", menuName = "Data/PatternSO")]
public class PatternSO : ScriptableObject
{
    public List<Pattern> dataList = new List<Pattern>();

    public Pattern GetById(int id)
        => dataList.Find(x => x.pattern_id == id);
}
