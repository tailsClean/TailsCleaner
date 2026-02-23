using UnityEngine;
using MonsterEnum;

public class monsterTable : ScriptableObject
{
    public int monsterId;

    public string monsterName;

    public MonsterMove monsterMove;
    public MonsterType monsterType;

    public int stageId;
    public int patternGroupId;
}
