
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharIngameTableSO", menuName = "Data/CharIngameTableSO")]
public class CharIngameTableSO : ScriptableObject
{
    public List<CharIngameTable> dataList = new List<CharIngameTable>();

    public CharIngameTable GetById(int id)
        => dataList.Find(x => x.char_ingame_level == id);
}
