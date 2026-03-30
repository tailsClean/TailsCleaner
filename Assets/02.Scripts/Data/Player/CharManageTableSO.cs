
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharManageTableSO", menuName = "Data/CharManageTableSO")]
public class CharManageTableSO : ScriptableObject
{
    public List<CharManageTable> dataList = new List<CharManageTable>();

    public CharManageTable GetById(int id)
        => dataList.Find(x => x.char_level == id);
}
