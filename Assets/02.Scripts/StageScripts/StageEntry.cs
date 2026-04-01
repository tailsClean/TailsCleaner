using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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

    [SerializeField] private StageMapLoader _mapLoader;

    [Header("Loading")]
    [SerializeField] private GameObject _loadingPanelPrefab;
    [SerializeField] private float _minimumLoadingSeconds = 3f;
    [SerializeField] private PlayerBase _player;

    private GameObject _loadingPanelInstance;
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

    private async void Start()
    {
        if (_stageController != null)
        {
            _stageController.SetGameplayBlocked(true);
        }

        if (_player != null)
        {
            _player.ForceStopMovement();
        }

        ShowLoadingPanel();

        float startTime = Time.unscaledTime;

        StagePlan plan = _planProvider.GetStagePlan(_stageId);
        if (plan == null)
        {
            HideLoadingPanel();

            if (_stageController != null)
                _stageController.SetGameplayBlocked(false);

            Debug.LogError($"[StageEntry] StagePlan load failed. stageId={_stageId}");
            return;
        }

        //Debug.Log($"[StageEntry] mapResource={plan.mapResource}");

        if (_mapLoader != null)
        {
            await _mapLoader.LoadMap(plan.mapResource);
        }
        else
        {
            Debug.LogWarning("[StageEntry] StageMapLoader is null.");
        }

        if (_useTimeOverride)
        {
            plan.mainLimitSeconds = _overrideMainTimeSeconds;
            plan.bossLimitSeconds = _overrideBossTimeSeconds;
        }

        ApplyTowerModifier(plan, _stageId);

        float elapsed = Time.unscaledTime - startTime;
        float remain = _minimumLoadingSeconds - elapsed;

        if (remain > 0f)
        {
            await Task.Delay(Mathf.CeilToInt(remain * 1000f));
        }

        HideLoadingPanel();

        if (_stageController == null)
        {
            Debug.LogError("[StageEntry] StageController is null.");
            return;
        }

        _stageController.StartStage(plan, _spawner, _register);
    }

    private void ShowLoadingPanel()
    {
        if (_loadingPanelPrefab == null)
        {
            Debug.LogWarning("[StageEntry] Loading panel prefab is null.");
            return;
        }

        if (_loadingPanelInstance != null)
            return;

        _loadingPanelInstance = Instantiate(_loadingPanelPrefab);
        _loadingPanelInstance.name = $"{_loadingPanelPrefab.name}_Instance";

        //Debug.Log("[StageEntry] Loading panel shown.");
    }

    private void HideLoadingPanel()
    {
        if (_loadingPanelInstance != null)
        {
            Destroy(_loadingPanelInstance);
            _loadingPanelInstance = null;
            //Debug.Log("[StageEntry] Loading panel hidden.");
        }
    }

    private void ApplyStageFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[StageEntry] GameManager 없음");
            return;
        }

        var gm = GameManager.Instance;

        if (gm._currentTower != null)
        {
            _towerId = gm._currentTower.tower_id;
        }

        if (gm._currentStage != null)
        {
            _stageId = gm._currentStage.stage_id;
            //Debug.Log($"[StageEntry] stageId ← GameManager(stage) = {_stageId}");
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
        List<TowerTableRow> towers = DataParser.Parse<TowerTableRow>("stage/tower_table");
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
            {
                if (towers[i].tower_id == _towerId)
                {
                    selected = towers[i];
                    break;
                }
            }
        }
        else
        {
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
}