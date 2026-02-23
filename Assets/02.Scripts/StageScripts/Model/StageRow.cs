using UnityEngine;

public sealed class StageTableRow
{
    public int stage_id;
    public int main_time;
    public int boss_time;
    public int monster_group_id;
    public int boss_id;
}

public sealed class MonsterWaveRow
{
    public int group_id;
    public int wave_index;
    public int start_time;
    public int end_time;

    public int monster_id;
    public int spawn_amount;
    public SpawnPattern spawn_pattern;

    public int mid_boss_id;

    // 없으면 CSV에 추가하거나, Builder에서 기본값으로 처리
    public int weight_percent;
}
