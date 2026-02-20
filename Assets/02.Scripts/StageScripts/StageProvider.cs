using System.Collections.Generic;
using UnityEngine;

public interface IStagePlanProvider
{
    StagePlan GetStagePlan(int _stageId);
}

//CSV 파일에서 스테이지 계획을 읽어오는 구현체
public sealed class DataParserStagePlanProvider : IStagePlanProvider
{
    private const string STAGE_TABLE_FILE = "stage_table";
    private const string WAVE_TABLE_FILE = "monster_wave_table";

    private readonly StagePlanBuilder _builder = new StagePlanBuilder();

    public StagePlan GetStagePlan(int _stageId)
    {
        List<StageTableRow> _stages = DataParser.Parse<StageTableRow>(STAGE_TABLE_FILE);
        List<MonsterWaveRow> _waves = DataParser.Parse<MonsterWaveRow>(WAVE_TABLE_FILE);

        if (_stages == null || _waves == null)
        {
            Debug.LogError("[StagePlanProvider] CSV parse failed.");
            return null;
        }

        StageTableRow _stage = null;
        for (int i = 0; i < _stages.Count; i++)
        {
            if (_stages[i].stage_id == _stageId)
            {
                _stage = _stages[i];
                break;
            }
        }

        if (_stage == null)
        {
            Debug.LogError($"[StagePlanProvider] stage_id not found: {_stageId}");
            return null;
        }

        List<MonsterWaveRow> _group = new List<MonsterWaveRow>();
        for (int i = 0; i < _waves.Count; i++)
        {
            if (_waves[i].group_id == _stage.monster_group_id)
                _group.Add(_waves[i]);
        }

        if (_group.Count == 0)
        {
            Debug.LogError($"[StagePlanProv" +
                $"ider] wave group not found: group_id={_stage.monster_group_id}");
            return null;
        }

        return _builder.Build(_stage, _group);
    }
}