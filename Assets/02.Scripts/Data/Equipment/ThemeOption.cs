
using System;

public enum OPS_STAT //옵션 스탯 분류
{
    Attack = 0, //공격력
    AttackSpeed = 1, //공격 속도
    Defense = 2, //방어력
    MoveSpeed = 3, //이동 속도
    Regeneration = 4, //회복효과
    Shield  = 5 //방어막
}

public enum TRIGGER_TYPE //옵션 발동 조건
{
    Regenerate = 0, //캐릭터가 회복효과를 받을 때
    GetShield = 1, //캐릭터가 보호막을 얻을 때
    AttackMonster = 2, //캐릭터 공격이 몬스터에게 적용되었을 때
    EvadeAttack = 3, //캐릭터의 회피가 발동되었을 때 
}

[Serializable]
public class ThemeOption
{
    public int id;
    public string desc;
    public int optiongroup_id;
    public OPS_STAT type;
    public TRIGGER_TYPE trigger;
    public bool is_buff;
    public float value;
    public int value_time;
    public int react_time;
}
