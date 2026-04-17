using UnityEngine;

// 스테이지 메인 전투 상태.
// - 메인 타이머를 시작하고
// - 메인타이머(초)를 기준으로 WaveScheduler를 Tick하여 웨이브 적용/중간보스 트리거를 수행한다.
// (타이머 진행 자체는 StageController에서 단일 Tick으로 관리)
public class CombatState : IStageState
{
    private readonly StageTimer _timer;
    private readonly WaveScheduler _waveScheduler;

    public CombatState(StageTimer timer, WaveScheduler waveScheduler)
    {
        _timer = timer;
        _waveScheduler = waveScheduler;
    }

    public void Enter()
    {
        _timer.ResetMain();
        _timer.StartMain();
    }
    
    public void Tick(float _deltaTime)
    {
        int _mainSeconds = _timer.GetMainTimeSecondsInt();
        _waveScheduler.Tick(_mainSeconds);
    }

    public void Exit()
    {
        _timer.StopMain();
    }
}
