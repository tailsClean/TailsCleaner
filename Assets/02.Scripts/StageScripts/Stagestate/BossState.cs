using System.Collections;
using UnityEngine;

//보스 스테이지 상태
//필드 몬스터 정리, 보스 소환, 보스 타이머 시작

public class BossState : IStageState
{
    private StageController _controller;
    private StageTimer _timer;
    private IMonsterRegistry _registry;
    private IMonsterSpawnSystem _spawner;


    private int _bossId;

    public BossState(StageController controller, StageTimer timer, IMonsterRegistry registry, IMonsterSpawnSystem spawner, int bossId)
    {
        _controller = controller;
        this._timer = timer;
        this._registry = registry;
        this._spawner = spawner;
        this._bossId = bossId;                // 보스 타이머 시작
    }

    public void Enter()
    {
        if (_controller == null)
        {
            Debug.LogError("[BossState] controller is null.");
            return;
        }

        _controller.StartCoroutine(CoEnterBossWave());
    }

    public void Tick(float deltaTime)
    {
    }

    public void Exit()
    {
        _timer.StopBoss();
        _registry.ClearBossMark();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeStateBossHP();
        }
    }

    private IEnumerator CoEnterBossWave()
    {
        Debug.Log("[BossState] CoEnterBossWave START");
        Debug.Log($"[BossState] banner ref null? {UIManager.Instance == null || UIManager.Instance.StageWaveBanner == null}");
        Debug.Log("[BossState] before PlayBossIntro");

        if (_registry == null)
        {
            Debug.LogError("[BossState] registry is null.");
            yield break;
        }

        if (_spawner == null)
        {
            Debug.LogError("[BossState] spawner is null.");
            yield break;
        }

        if (_timer == null)
        {
            Debug.LogError("[BossState] timer is null.");
            yield break;
        }

        _spawner.SetSpawningEnabled(false);

        MonsterRegistry registryImpl = _registry as MonsterRegistry;
        if (registryImpl != null)
        {
            // 현재 필드 몬스터들 정지
            registryImpl.SetAllMonstersPaused(true, includeBoss: true);
        }

        GameObject spawnedBoss = null;

        if (UIManager.Instance != null && UIManager.Instance.StageWaveBanner != null)
        {
            yield return _controller.StartCoroutine(
    UIManager.Instance.StageWaveBanner.PlayBossIntro(
        "엄청 꼬질한 녀석이 나타났어요!",
        () =>
        {
            if (registryImpl != null)
            {
                registryImpl.KillAllMonsters(includeBoss: false);
            }
            else
            {
                _registry.KillAllMonsters();
            }

            _spawner.SpawnBoss(_bossId);

            if (_registry is MonsterRegistry mr && _spawner is RuleBasedMonsterSpawner rb)
            {
                spawnedBoss = rb.LastSpawnedBoss;
                mr.MarkBoss(spawnedBoss);

                if (spawnedBoss != null && spawnedBoss.TryGetComponent<MonsterBase>(out var bossBase))
                {
                    bossBase.SetPaused(true);
                }
            }
        })
);
        }
        else
        {
            if (registryImpl != null)
            {
                registryImpl.KillAllMonsters(includeBoss: false);
            }
            else
            {
                _registry.KillAllMonsters();
            }

            _spawner.SpawnBoss(_bossId);

            if (_registry is MonsterRegistry mr && _spawner is RuleBasedMonsterSpawner rb)
            {
                spawnedBoss = rb.LastSpawnedBoss;
                mr.MarkBoss(spawnedBoss);

                if (spawnedBoss != null && spawnedBoss.TryGetComponent<MonsterBase>(out var bossBase))
                {
                    bossBase.SetPaused(true);
                }
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ChangeStateBossHP();
        }

        // 연출 종료 후 보스만 해제
        if (spawnedBoss != null && spawnedBoss.TryGetComponent<MonsterBase>(out var spawnedBossBase))
        {
            spawnedBossBase.SetPaused(false);
        }

        _timer.StartBoss();
    }
}