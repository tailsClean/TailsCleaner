using System;
using MonsterEnum;



[Serializable]
public class PatternGroup
{
    public int pattern_group_id;
    public string index;
    public int max_concurrent_pattern;
    public OVERLAPRULE overlap_rule;
}
