
using System;


public enum GRADE
{
    Dirty = 0,      // 꼬질한
    Normal = 1,     // 평범한
    Aromatic = 2,   // 향긋한
    Clean = 3,      // 깨끗한
    Shiny = 4,      // 눈부신
    None            // 등급 없음
}

[Serializable]
public class EquipGrade
{
    public int id;
    public string desc;
    public int group_id;
    public GRADE grade;
    public bool is_max_grade;
    public int cost_id;
    public int cost_count;
    public float stat_rate;
}
