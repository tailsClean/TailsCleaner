// 자동 생성 됨.
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterWaveTableSO", menuName = "Data/MonsterWaveTableSO")]
public class MonsterWaveTableSO : ScriptableObject
{
    public List<MonsterWaveTable> dataList = new List<MonsterWaveTable>();

    public MonsterWaveTable GetById(int id)
        => dataList.Find(x => x.wave_id == id);
}
