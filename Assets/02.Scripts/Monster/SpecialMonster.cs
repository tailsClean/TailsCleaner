using UnityEngine;
using MonsterEnum;

public class SpecialMonster : SpecialBossMonsterBase
{
    public override MONSTERTYPE monsterType => MONSTERTYPE.Special;

    protected override void Start()
    {
        base.Start();
    }
}
