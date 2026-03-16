using System;
using MonsterEnum;


[Serializable]
public class Monster
{
    public int monster_id;
    public string index;
    public MONSTERTYPE monster_type;
    public int pattern_group_id;
    public float drop_chance;
    public int drop_group_id;
    public int daily_gold_drop;
    public int drop_resource_id;
    public int resource_id;
}
