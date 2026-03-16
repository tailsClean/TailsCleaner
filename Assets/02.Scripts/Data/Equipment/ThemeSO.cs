
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ThemeSO", menuName = "Data/ThemeSO")]
public class ThemeSO : ScriptableObject
{
    public List<Theme> dataList = new List<Theme>();

    public Theme GetById(int id)
        => dataList.Find(x => x.id == id);
}
