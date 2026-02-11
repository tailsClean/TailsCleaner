using System;

public class StageEvents
{
    // 스테이지 시간을 진행을 위한 이벤트
    public event Action<int> OnMainSecondTick;
    public event Action OnMainTimerReachedLimit;

    // 보스 타이머 관련 이벤트 -> 이후 삭제될 수도 있음
    public event Action<int> OnBossSecondTick;
    public event Action OnBossTimerExpired;

    //웨이브 바뀌는 이벤트
    public event Action<int> OnWaveChanged;

    public void RaiseMainSecondTick(int _seconds) => OnMainSecondTick?.Invoke(_seconds);
    public void RaiseMainTimerReachedLimit() => OnMainTimerReachedLimit?.Invoke();

    public void RaiseBossSecondTick(int _secondsLeft) => OnBossSecondTick?.Invoke(_secondsLeft);
    public void RaiseBossTimerExpired() => OnBossTimerExpired?.Invoke();

    public void RaiseWaveChanged(int _waveIndex) => OnWaveChanged?.Invoke(_waveIndex);
}
