
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatternObjectResourceSO", menuName = "Data/PatternObjectResourceSO")]
public class PatternObjectResourceSO : ScriptableObject
{
    public List<PatternObjectResource> dataList = new List<PatternObjectResource>();

    public PatternObjectResource GetById(int id)
        => dataList.Find(x => x.resource_id == id);
}
