using UnityEngine;

public sealed class StageTableRow
{
    public int stage_id;
    public int tower_id;
    public int stage_index;

    public int main_time;
    public int boss_time;

    public int entry_energy;

    public int monster_group_id;
    public int special_group_id;
    public int boss_id;

    public float hp_modifier;
    public float power_modifier;
}

public sealed class MonsterWaveRow
{
    public int wave_id;
    public string _description;

    public int group_id;
    public int wave_index;
    public int start_time;
    public int end_time;

    public int monster_id;
    public int spawn_amount;
    public SpawnPattern spawn_pattern;

    public int mid_boss_id;

    public float hp_modifier;
    public float power_modifier;
    public float exp_multiply;
}

public sealed class SpecialMonsterRow
{
    public int special_id;
    public string _description;

    public int special_group_id;
    public int spawn_type; // CSV가 enum이긴 한데, 숫자로 올 가능성이 높아서 int로 받는게 안전

    public int start_time;
    public int end_time;

    public int generation_time; // 생성 주기 -- spawn_type이 Periodic일 때, 이 시간마다 반복해서 소환한다는 의미(초 단위)
    public int monster_id;
}

public sealed class TowerTableRow
{
    public int tower_id;
    public string tower_name_key;
    public int need_stage_id;
    public float hp_modifier;
    public float power_modifier;
    public string tower_icon_resource;
    public string bgm_resource;
}

// monster_table
public sealed class MonsterTableRow
{
    public int monster_id;
    public string index;
    public int monster_type; // 0=일반,1=특수,2=중간보스,3=보스
    public int pattern_group_id;
    public float drop_chance;
    public int drop_group_id;
    public int resource_id;
}

// monster_type_table
public sealed class MonsterTypeTableRow
{
    public int monster_type; // 0~3
    public float type_hp_multiply;
    public float type_power_multiply;
    public float type_move_speed;
    public float type_mass;
    public float type_hit_box;
    public float type_nb;
    public float base_exp; 
    public string exp_resource_id;
}

public enum SpecialSpawnType
{
    Periodic = 1, // 일정 주기 반복
    Once = 2,     // 1회성
}