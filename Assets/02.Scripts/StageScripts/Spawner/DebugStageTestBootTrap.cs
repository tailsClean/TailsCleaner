using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 파서/데이터매니저가 완성되기 전까지 스테이지 흐름을 검증하기 위한 부트스트랩.
/// - 씬 시작 시 StagePlan을 하드코딩으로 생성하여 StageController.StartStage를 호출한다.
/// - 파서가 들어오면 이 스크립트만 제거하면 된다.
/// </summary>
public sealed class DebugStageTestBootstrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private StageController _stageController;
    [SerializeField] private MonoBehaviour _spawnerBehaviour;
    [SerializeField] private MonoBehaviour _registryBehaviour;

    [Header("Test Time Settings")]
    [SerializeField] private int _mainLimitSeconds = 30;
    [SerializeField] private int _bossLimitSeconds = 10;

    private void Start()
    {
        IMonsterSpawnSystem _spawner = _spawnerBehaviour as IMonsterSpawnSystem;
        IMonsterRegistry _registry = _registryBehaviour as IMonsterRegistry;

        if (_stageController == null || _spawner == null || _registry == null)
        {
            Debug.LogError("[Bootstrap] Missing references. Assign StageController/Spawner/Registry in Inspector.");
            return;
        }

        StagePlan _plan = CreateTestPlan(_mainLimitSeconds, _bossLimitSeconds);

        Debug.Log($"[Bootstrap] StartStage: stageId={_plan.stageId}, main={_plan.mainLimitSeconds}s, boss={_plan.bossLimitSeconds}s, bossId={_plan.bossId}, waves={_plan.wavePlans.Count}");
        _stageController.StartStage(_plan, _spawner, _registry);
    }

    private StagePlan CreateTestPlan(int _mainLimitSeconds, int _bossLimitSeconds)
    {
        List<WavePlan> _waves = new List<WavePlan>
        {
            new WavePlan
            {
                waveIndex = 1,
                startTimeSeconds = 0,
                endTimeSeconds = 10,
                spawnPattern = SpawnPattern.Random,
                midBossId = 0,
                spawns = new List<MonsterSpawnPlan>
                {
                    new MonsterSpawnPlan { monsterId = 1, spawnAmount = 10, spawnIntervalSeconds = 0.5f, weightPercent = 100 }
                }
            },
            new WavePlan
            {
                waveIndex = 2,
                startTimeSeconds = 10,
                endTimeSeconds = 20,
                spawnPattern = SpawnPattern.Squad,
                midBossId = 101,
                spawns = new List<MonsterSpawnPlan>
                {
                    new MonsterSpawnPlan { monsterId = 2, spawnAmount = 7, spawnIntervalSeconds = 0.5f, weightPercent = 70 },
                    new MonsterSpawnPlan { monsterId = 3, spawnAmount = 3, spawnIntervalSeconds = 0.5f, weightPercent = 30 },
                }
            },
            new WavePlan
            {
                waveIndex = 3,
                startTimeSeconds = 20,
                endTimeSeconds = 30,
                spawnPattern = SpawnPattern.Circle,
                midBossId = 0,
                spawns = new List<MonsterSpawnPlan>
                {
                    new MonsterSpawnPlan { monsterId = 4, spawnAmount = 10, spawnIntervalSeconds = 0.2f, weightPercent = 100 },
                }
            },
        };

        return new StagePlan
        {
            stageId = 1,
            mainLimitSeconds = _mainLimitSeconds,
            bossLimitSeconds = _bossLimitSeconds,
            bossId = 999,
            wavePlans = _waves
        };
    }
}