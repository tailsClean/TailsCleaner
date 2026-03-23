using System.Collections.Generic;
using UnityEngine;

public interface IStagePlanProvider
{
    StagePlan GetStagePlan(int _stageId);
}

// CSV 파일에서 스테이지 계획을 읽어오는 구현체
public sealed class DataParserStagePlanProvider : IStagePlanProvider
{
    private const string STAGE_TABLE_FILE = "stage/stage_table";
    private const string WAVE_TABLE_FILE = "stage/monster_wave_table";
    private const string SPECIAL_TABLE_FILE = "stage/special_monster_table";

    private readonly StagePlanBuilder _builder = new StagePlanBuilder();

    public StagePlan GetStagePlan(int _stageId)
    {
        List<StageTableRow> _stages = DataParser.Parse<StageTableRow>(STAGE_TABLE_FILE);
        List<MonsterWaveRow> _waves = DataParser.Parse<MonsterWaveRow>(WAVE_TABLE_FILE);
        List<SpecialMonsterRow> _specialRows = DataParser.Parse<SpecialMonsterRow>(SPECIAL_TABLE_FILE);

        if (_stages == null || _waves == null || _specialRows == null)
        {
            Debug.LogError(
                $"[StagePlanProvider] CSV parse failed. " +
                $"stagesNull={_stages == null}, wavesNull={_waves == null}, specialNull={_specialRows == null}"
            );
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

        List<SpecialMonsterRow> specialGroup = new List<SpecialMonsterRow>();
        for (int i = 0; i < _specialRows.Count; i++)
        {
            if (_specialRows[i].special_group_id == _stage.special_group_id)
                specialGroup.Add(_specialRows[i]);
        }

        if (_group.Count == 0)
        {
            Debug.LogError(
                $"[StagePlanProvider] wave group not found: group_id={_stage.monster_group_id}, stageId={_stageId}"
            );
            return null;
        }

        StagePlan builtPlan = _builder.Build(_stage, _group, specialGroup);

        if (builtPlan == null)
        {
            Debug.LogError($"[StagePlanProvider] StagePlan build failed. stageId={_stageId}");
            return null;
        }

        Debug.Log(
            $"[StagePlanProvider] StagePlan built. " +
            $"stageId={builtPlan.stageId}, waves={builtPlan.wavePlans?.Count ?? 0}, " +
            $"specialRows={builtPlan.specialRows?.Count ?? 0}, bossId={builtPlan.bossId}"
        );

        return builtPlan;
    }
}