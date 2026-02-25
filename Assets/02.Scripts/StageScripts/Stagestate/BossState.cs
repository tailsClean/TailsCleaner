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

        _spawner.SetSpawningEnabled(false);
        //필드 몬스터 정리
        _registry.KillAllMonsters();
        //보스 소환
        _spawner.SpawnBoss(_bossId);
        
        if (_registry is MonsterRegistry mr && _spawner is RuleBasedMonsterSpawner rb)
        {
            mr.MarkBoss(rb.LastSpawnedBoss);
        }

        //보스 타이머 시작
        _timer.StartBoss();
    }

    public void Tick(float _deltatime)
    {
    }

    public void Exit()
    {
        _timer.StopBoss();
    }
}
