using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicSO", menuName = "Data/RelicSO")]
public class RelicSO : ScriptableObject
{
    public List<Relic> dataList = new List<Relic>();

    public Relic GetById(int id)
        => dataList.Find(x => x.id == id);
}
