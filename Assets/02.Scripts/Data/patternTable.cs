using UnityEngine;
using MonsterEnum;

public class patternTable : ScriptableObject
{
    public int patternId;

    public PatternType patternType;

    public float castTime;
    public float duration;
    public float range;
    public float powerScale;

    public DebuffType debuffType;

    public float debuffValue;
}
