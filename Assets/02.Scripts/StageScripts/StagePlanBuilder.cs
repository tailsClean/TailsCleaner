using System.Collections.Generic;
using UnityEngine;

public class StagePlanBuilder
{
    private const int DEFAULT_WEIGHT = 100; // 기본값으로 사용할 가중치
    private const int NO_BOSS_ID = -1; // 보스가 없는 경우의 ID

    public StagePlan Build(StageTableRow _stage, List<MonsterWaveRow> _waveRows)
    {
        if (_stage == null || _waveRows == null)
            return null;

        Dictionary<int, List<MonsterWaveRow>> _byWave = new Dictionary<int, List<MonsterWaveRow>>();
        for (int i = 0; i < _waveRows.Count; i++)
        {
            MonsterWaveRow _r = _waveRows[i];
            if (!_byWave.ContainsKey(_r.wave_index))
                _byWave[_r.wave_index] = new List<MonsterWaveRow>();

            _byWave[_r.wave_index].Add(_r);
        }

        List<WavePlan> _waves = new List<WavePlan>();
        foreach (KeyValuePair<int, List<MonsterWaveRow>> _pair in _byWave)
        {
            List<MonsterWaveRow> _rows = _pair.Value;
            MonsterWaveRow _first = _rows[0];

            List<MonsterSpawnPlan> _spawns = new List<MonsterSpawnPlan>();
            for (int i = 0; i < _rows.Count; i++)
            {
                MonsterWaveRow _row = _rows[i];
                int _weight = _row.weight_percent > 0 ? _row.weight_percent : DEFAULT_WEIGHT;

                _spawns.Add(new MonsterSpawnPlan
                {
                    monsterId = _row.monster_id,
                    spawnAmount = _row.spawn_amount,
                    spawnIntervalSeconds = 0f, // 스폰 주기는 "초당 5마리" 규칙으로 스포너가 관리
                    weightPercent = _weight
                });
            }

            int _midBoss = _first.mid_boss_id;
            if (_midBoss == 0)
                _midBoss = NO_BOSS_ID;

            _waves.Add(new WavePlan
            {
                waveIndex = _first.wave_index,
                startTimeSeconds = _first.start_time,
                endTimeSeconds = _first.end_time,
                spawnPattern = _first.spawn_pattern,
                midBossId = _midBoss,
                spawns = _spawns
            });
        }

        _waves.Sort((a, b) => a.waveIndex.CompareTo(b.waveIndex));

        return new StagePlan
        {
            stageId = _stage.stage_id,
            mainLimitSeconds = _stage.main_time,
            bossLimitSeconds = _stage.boss_time,
            bossId = _stage.boss_id,
            wavePlans = _waves
        };
    }
}
