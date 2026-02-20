using UnityEngine;
using MonsterEnum;

public class SpecialMonster : SpecialBossMonsterBase
{
    public override MonsterType monsterType => MonsterType.Special;

    protected override void Start()
    {
        base.Start();
    }
}
