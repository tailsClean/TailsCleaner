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

    public int reward_group_id;
    public int rank_table_id;

    public float hp_modifier;
    public float power_modifier;

    public int reward_preview;
    public string map_resource;
    public int exp_gain;
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

public sealed class PatternGroupTableRow
{
    public int pattern_group_id;
    public string index;
    public int max_concurrent_pattern;
    public int overlap_rule;
}

public sealed class PatternGroupCompositionTableRow
{
    public int pattern_group_id;
    public string index;
    public int pattern_id;
    public float pattern_cooldown;
    public int priority;
}

public sealed class PatternTableRow
{
    public int pattern_id;
    public string index;
    public int resource_id;
    public int pattern_type;
    public string pattern_logic_type;

    public int stat_target;
    public float stat_value;

    public float duration;
    public float cast_time;
    public float damage_multiply;
    public float detect_range;
    public float move_time;
    public float zigzag_width;
    public float jump_height;

    public int projectile_type;
    public float projectile_speed;
    public float projectile_size;
    public int projectile_count;
    public float projectile_radius;
    public float fire_interval;
    public float life_time;
    public int pierce_type;
    public bool follow;
    public float arc_height;

    public float area_radius;
    public float area_damage_interval;
    public int area_target_type;
    public float explode_range;

    public int summon_monster_id;
    public int summon_count;
    public float summon_radius;
    public int summon_position_type;

    public int debuff_type;
    public float debuff_duration;
    public float debuff_value;
    public float debuff_damage_interval;

    public int barrier_target_type;
    public int barrier_shape;
    public float barrier_size_x;
    public float barrier_size_y;
    public float barrier_damage;
    public bool barrier_collision_block;

    public float dirty_to_hp_value;
    public float enrage_time;
    public float enrage_atk_rate;
    public float enrage_hp_rate;
    public int enrage_max_step;
    public int item_id;
}

public enum SpecialSpawnType
{
    Periodic = 0, // 일정 주기 반복
    Once = 1,     // 1회성
}