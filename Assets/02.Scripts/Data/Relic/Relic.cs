using System;


public enum STAT_TYPE
{
    Gold = 0,
    Equipment = 1,
    Exp = 2,
    ItemRange = 3,
}

// TODO: 'RelicType' 멤버 이름을 실제 값으로 수정하세요.
public enum RELIC_TYPE
{
    Twinkle = 0, //반짝반짝
    Smooth= 1,
    Swish= 2,
    Floating = 3,
}

[Serializable]
public class Relic
{
    public int id;
    public string desc;
    public STAT_TYPE stat_type;
    public float stat_value;
    public int group_id;
    public RELIC_TYPE relic_type;
    public string name;
    public string script;
    public string sprite;
    public string effect;
    public string sound;
}
