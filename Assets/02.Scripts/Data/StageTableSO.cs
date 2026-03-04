// 자동 생성 됨.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageTableSO", menuName = "Data/StageTableSO")]
public class StageTableSO : ScriptableObject
{
    public List<StageTable> dataList = new List<StageTable>();

    public StageTable GetById(int id)
        => dataList.Find(x => x.stage_id == id);
}
