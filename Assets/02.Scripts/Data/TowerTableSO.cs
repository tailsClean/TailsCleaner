// 자동 생성 됨.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerTableSO", menuName = "Data/TowerTableSO")]
public class TowerTableSO : ScriptableObject
{
    public List<TowerTable> dataList = new List<TowerTable>();

    public TowerTable GetById(int id)
        => dataList.Find(x => x.tower_id == id);
}
