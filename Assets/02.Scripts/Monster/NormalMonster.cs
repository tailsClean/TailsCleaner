using UnityEngine;
using MonsterEnum;
public class NormalMonster : MonsterBase
{
    public override MonsterType monsterType => MonsterType.Normal;
    protected override void Awake()
    {
        // MonsterBase의 Awake 실행
        base.Awake();

        hp = 1.0f;
        mass = 1.0f;
        moveSpeed = 1.0f;
    }
}