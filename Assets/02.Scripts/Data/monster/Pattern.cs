using System;

public enum PATTERN_TYPE
{
    None = 0, //없음
    Move = 1, //이동
    Projectile = 2, //투사체
    Area = 3, //영역
    Summon = 4, //소환수
    Trigger = 5, //트리거 
    SelfDestruct = 6, //자폭
    Layser = 7, //레이저
    Barricade = 8, //바리케이드
}

public enum STAT_TARGET
{
    HP = 0,
    Power = 1,
    Move_Speed= 2,
    Mass = 3,
    Hitbox = 4,
    KnockBack = 5, //csv상 nb
    Base_EXP = 6
}

public enum ESCAPE_TARGET
{
    Reverse = 0, //플레이어 반대 방향
    Crowd = 1, //밀집 지역
    Target_Location = 2, //지정 위치
}

public enum PROJECTILE_TYPE
{
    Straight = 0, //직선
    Parabola = 1, //포물선 
    Turn = 2, //공전
    Layser = 3 //레이저
}

public enum PIERCE_TYPE
{
    Extinction = 0, //소멸
    Piece = 1, //관통
    Reflect = 2, //반사 
}

public enum AREA_TARGET_TYPE
{
    Player = 0,
    Boss = 1,
}

public enum SUMMON_POSITION_TYPE
{
    Player = 0,
    Boss = 1

}

public enum DEBUFF_TYPE
{
    DOT = 0, //지속피해 
    Stun = 1,
    Slow = 2, //이동 속도 감소 
}

public enum BARRIER_TARGET_TYPE
{
    Player = 0, //플레이어만
    Boss= 1, //보스만
    Both = 2, //플레이어 보스 둘 다
    Nobody = 3 //아무도 없는 위치
}

public enum BarrierShapeType
{
    Rectangular = 0,
    Circle = 1
}

[Serializable]
public class Pattern
{
    public int pattern_id;
    public string index;
    public int resource_id;
    public PATTERN_TYPE pattern_type;
    public string pattern_logic_type;
    public STAT_TARGET stat_target;
    public float stat_value;
    public float duration;
    public float cast_time;
    public float cooldown;
    public float damage_multiply;
    public float detect_range;
    public float move_time;
    public float zigzag_width;
    public float jump_height;
    public float chase_time;
    public ESCAPE_TARGET escape_target;
    public float rush_speed;
    public PROJECTILE_TYPE projectile_type;
    public float projectile_speed;
    public float projectile_size;
    public int projectile_count;
    public float fire_interval;
    public float life_time;
    public PIERCE_TYPE pierce_type;
    public bool follow;
    public float arc_height;
    public float projectile_radius;
    public float area_radius;
    public float area_damage_interval;
    public AREA_TARGET_TYPE area_target_type;
    public float explode_range;
    public int summon_monster_id;
    public int summon_count;
    public float summon_radius;
    public SUMMON_POSITION_TYPE summon_position_type;
    public DEBUFF_TYPE debuff_type;
    public float debuff_duration;
    public float debuff_value;
    public float debuff_damage_interval;
    public BARRIER_TARGET_TYPE barrier_target_type;
    public BarrierShapeType barrier_shape;
    public float barrier_size_x;
    public float barrier_size_y;
    public float barrier_damage;
    public bool barrier_collision_block;
}
