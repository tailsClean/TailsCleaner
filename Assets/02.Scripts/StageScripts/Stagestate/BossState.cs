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

        if (SoundManager.Instance) SoundManager.Instance.PlayBGM(BGMName.Boss_Normal, false);

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

        // 보스 등장 연출 중 게임플레이 차단 시작
        _controller.SetBossIntroPlaying(true);
        _controller.SetGameplayBlocked(true);

        MonsterRegistry registryImpl = _registry as MonsterRegistry;
        if (registryImpl != null)
        {
            registryImpl.SetAllMonstersPaused(true, includeBoss: true);
        }

        GameObject spawnedBoss = null;

        if (UIManager.Instance != null && UIManager.Instance.StageWaveBanner != null)
        {
            // 여기서는 "보스 소환"까지만 하고 exp absorb 즉시 실행은 하지 않음
            yield return _controller.StartCoroutine(
                UIManager.Instance.StageWaveBanner.PlayBossIntro(
                    "보스 등장!!!",
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

        // 연출 종료 후 보스 해제
        if (spawnedBoss != null && spawnedBoss.TryGetComponent<MonsterBase>(out var spawnedBossBase))
        {
            spawnedBossBase.SetPaused(false);
        }

        // 보스 등장 연출은 끝났으므로 Intro 상태 해제
        _controller.SetBossIntroPlaying(false);
        _controller.SetGameplayBlocked(false);

        // exp absorb 즉시 예고는 "보스 인트로가 완전히 끝난 뒤" 실행
        if (spawnedBoss != null)
        {
            var runner = spawnedBoss.GetComponent<BossTriggerPatternRunner>();
            if (runner != null)
            {
                runner.TryRunImmediateExpAbsorbPreview();
            }
        }

        _timer.StartBoss();
    }
}