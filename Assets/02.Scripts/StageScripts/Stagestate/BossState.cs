using UnityEngine;

//보스 스테이지 상태
//필드 몬스터 정리, 보스 소환, 보스 타이머 시작

public class BossState : IStageState
{
    private StageTimer _timer;
    private IMonsterRegistry _registry;
    private IMonsterSpawnSystem _spawner;


    private int _bossId;

    public BossState(StageTimer timer, IMonsterRegistry registry, IMonsterSpawnSystem spawner, int bossId)
    {
        this._timer = timer;
        this._registry = registry;
        this._spawner = spawner;
        this._bossId = bossId;                // 보스 타이머 시작
    }

    public void Enter()
    {
        if (_registry == null)
        {
            Debug.LogError("[BossState] registry is null.");
            return;
        }

        if (_spawner == null)
        {
            Debug.LogError("[BossState] spawner is null.");
            return;
        }

        if (_timer == null)
        {
            Debug.LogError("[BossState] timer is null.");
            return;
        }

        //필드 몬스터 정리
        _registry.KillAllMonsters();
        //보스 소환
        _spawner.SpawnBoss(_bossId);
        //보스 타이머 시작
        _timer.StartBoss();

        // 보스전 시작: 일반 스폰 끄기
        if (_spawner is RuleBasedMonsterSpawner ruleSpawner)
            ruleSpawner.SetSpawningEnabled(false);
    }

    public void Tick(float _deltatime)
    {
    }

    public void Exit()
    {
        // 보스전 종료: 일반 스폰 다시 켜기 (클리어/실패 처리 시점에서 호출되게)
        if (_spawner is RuleBasedMonsterSpawner ruleSpawner)
        {
            ruleSpawner.SetSpawningEnabled(true);
        }

        _timer.StopBoss();
    }
}
