using UnityEngine;
using MonsterEnum;
public class NormalMonster : MonsterBase
{
    protected override void Awake()
    {
        // MonsterBase의 Awake 실행
        base.Awake();

        // 몬스터 개별 스탯 설정
        monsterType = MonsterType.Normal;
        hp = 1.0f;
        mass = 1.0f;
        moveSpeed = 1.0f;
    }
}