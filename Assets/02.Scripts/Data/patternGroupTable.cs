using UnityEngine;
using MonsterEnum;
public class patternGroupTable : ScriptableObject
{
    public int patternGroupId;
    public int maxConcurrentPattern;

    public OverlapRule overlapRule;
}
