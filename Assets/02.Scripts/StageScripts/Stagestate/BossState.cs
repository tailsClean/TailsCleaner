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
        _timer = timer;
        _registry = registry;
        _spawner = spawner;
        _bossId = bossId;
    }

    public void Enter()
    {
        //필드 몬스터 정리
        _registry.KillAllMonsters();
        //보스 소환
        _spawner.SpawnBoss(_bossId);
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
