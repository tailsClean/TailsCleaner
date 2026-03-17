using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class StageEntry : MonoBehaviour
{
    [SerializeField] private int _stageId = 50201;
    [SerializeField] private int _towerId = 0;

    [SerializeField] private bool _useTimeOverride;
    [SerializeField] private int _overrideMainTimeSeconds = 60; 
    [SerializeField] private int _overrideBossTimeSeconds = 30; 

    [SerializeField] private StageController _stageController;
    [SerializeField] private RuleBasedMonsterSpawner _spawner;
    [SerializeField] private MonsterRegistry _register;

    private IStagePlanProvider _planProvider;

    private void Awake()
    {
        _planProvider = new DataParserStagePlanProvider();

        ApplyStageFromGameManager();

        if (_stageId <= 0)
        {
            Debug.LogError("[StageEntry] stageId가 0 → 잘못된 진입");
        }
    }

    void Start()
    {
        if (!TrySpendEntryEnergy(_stageId))
        { 
            // 에너지가 부족하면 타워씬으로 이동
            SceneManager.LoadScene("TowerScene");
            return;
        }


        StagePlan _plan = _planProvider.GetStagePlan(_stageId);
        if (_plan == null)
        {
            return;
        }

        if (_useTimeOverride)
        {
            _plan.mainLimitSeconds = _overrideMainTimeSeconds;
            _plan.bossLimitSeconds = _overrideBossTimeSeconds;
        }

        ApplyTowerModifier(_plan, _stageId);
        _stageController.StartStage(_plan, _spawner, _register);
    }

    private void ApplyStageFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[StageEntry] GameManager 없음");
            return;
        }

        var gm = GameManager.Instance;

        // tower
        if (gm._currentTower != null)
        {
            _towerId = gm._currentTower.tower_id;
        }

        // stageId (핵심)
        if (gm._currentStage != null)
        {
            _stageId = gm._currentStage.stage_id;
            Debug.Log($"[StageEntry] stageId ← GameManager(stage) = {_stageId}");
        }
        else if (gm._currentStageId > 0)
        {
            _stageId = gm._currentStageId;
            Debug.Log($"[StageEntry] stageId ← GameManager(stageId) = {_stageId}");
        }
        else
        {
            Debug.LogError("[StageEntry] GameManager에 stage 정보 없음 → Inspector 값 사용됨");
        }
    }

    private void ApplyTowerModifier(StagePlan plan, int stageId)
    {
        List<TowerTableRow> towers = DataParser.Parse<TowerTableRow>("tower_table");
        if (towers == null || towers.Count == 0)
        {
            plan.towerHpModifier = 0f;
            plan.towerPowerModifier = 0f;
            return;
        }

        TowerTableRow selected = null;

        if (_towerId > 0)
        {
            for (int i = 0; i < towers.Count; i++)
                if (towers[i].tower_id == _towerId) { selected = towers[i]; break; }
        }
        else
        {
            // 자동: need_stage_id <= stageId 중 가장 큰 need_stage_id를 가진 tower
            for (int i = 0; i < towers.Count; i++)
            {
                var t = towers[i];
                if (t.need_stage_id <= stageId)
                {
                    if (selected == null || t.need_stage_id > selected.need_stage_id)
                        selected = t;
                }
            }
        }

        if (selected == null)
        {
            plan.towerHpModifier = 0f;
            plan.towerPowerModifier = 0f;
            return;
        }

        plan.towerHpModifier = selected.hp_modifier;
        plan.towerPowerModifier = selected.power_modifier;
    }

    private bool TrySpendEntryEnergy(int stageId)
    {
        List<StageTableRow> stages = DataParser.Parse<StageTableRow>("stage_table");
        if (stages == null || stages.Count == 0)
        {
            Debug.LogWarning("[StageEntry] stage_table load failed. Fallback: allow entry.");
            return true; // 테이블 없으면 막지 않기
        }

        StageTableRow row = null;
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i].stage_id == stageId) { row = stages[i]; break; }
        }

        int cost = (row != null) ? Mathf.Max(0, row.entry_energy) : GameManager.SPEND_ENERGY;
        if (cost <= 0) return true;

        // 현재 에너지 확인
        if (GameManager.EnergyCount < cost)
        {
            Debug.Log($"에너지가 부족합니다. 현재={GameManager.EnergyCount}, 필요={cost}");
            return false;
        }
        return true;
    }
}
