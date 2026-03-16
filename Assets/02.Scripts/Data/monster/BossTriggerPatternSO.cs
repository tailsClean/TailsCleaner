
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BossTriggerPatternSO", menuName = "Data/BossTriggerPatternSO")]
public class BossTriggerPatternSO : ScriptableObject
{
    public List<BossTriggerPattern> dataList = new List<BossTriggerPattern>();

    public BossTriggerPattern GetById(int id)
        => dataList.Find(x => x.monster_id == id);
}
