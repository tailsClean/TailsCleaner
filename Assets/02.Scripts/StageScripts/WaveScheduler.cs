using UnityEngine;

//메인 타이머 기준 웨이브 흐름 제어 담당
// 웨이브 변경 감지 -> 이벤트 발행 및 스폰 시스템에 적용

public class WaveScheduler
{
    private const int INVALID_WAVE_INDEX = -1;
    private const int NO_BOSS_ID = 0;

    private readonly StageEvents _events;
    private readonly WaveTimeline _timeline;
    public readonly IMonsterSpawnSystem _spawner;

    private WavePlan _currentWave;
    private int _currentWaveIndex;

    // 웨이브 종료 트리거 중복 방지용
    private int _lastMidBossTriggeredWaveIndex;

    public WaveScheduler(StageEvents events, WaveTimeline timeline, IMonsterSpawnSystem spawner)
    {
        this._events = events;
        this._timeline = timeline;
        this._spawner = spawner;

        _currentWaveIndex = INVALID_WAVE_INDEX;
        _lastMidBossTriggeredWaveIndex = INVALID_WAVE_INDEX;
    }

    // 현재 시간(초)을 받아 웨이브 상태를 갱신한다.
    // StageController.Update에서 매 프레임(또는 매 초) 호출하는 것을 권장.
    public void Tick(int _mainSeconds)
    {

        WavePlan _wave = _timeline.GetWaveByTimeSeconds(_mainSeconds);
        if (_wave == null)
            return;

        // 웨이브 변경 감지
        if (_currentWaveIndex != _wave.waveIndex)
        {
            WavePlan _previousWave = _currentWave;

            _currentWave = _wave;
            _currentWaveIndex = _wave.waveIndex;

            // 웨이브 변경 이벤트 + 스폰 구성 적용
            _events.RaiseWaveChanged(_currentWaveIndex);
            _spawner.ApplyWave(_currentWave);

            // 웨이브 변경 시점에 “이전 웨이브 종료”를 처리할 수도 있지만,
            // 기획서가 end_time 기준을 강조하는 경우를 대비해 아래 방식 사용:
            TryTriggerMidBossByEndTime(_previousWave, _mainSeconds);
        }
        else
        {
            // 같은 웨이브 내에서도 end_time 통과 시점을 감지할 수 있도록
            // (프레임 드랍 등으로 변경 순간을 놓치는 케이스 방지)
            TryTriggerMidBossByEndTime(_currentWave, _mainSeconds);
        }

    }

    private void TryTriggerMidBossByEndTime(WavePlan _wave, int _mainSeconds)
    {
        if (_wave == null)
            return;

        int _lastWaveIndex = _timeline.GetLastWaveIndex();
        if (_wave.waveIndex == _lastWaveIndex)
            return;

        if (_wave.midBossId == NO_BOSS_ID)
            return;

        // end_time을 “통과한 순간”에만 1회 트리거
        if (_mainSeconds >= _wave.endTimeSeconds && _lastMidBossTriggeredWaveIndex != _wave.waveIndex)
        {
            _lastMidBossTriggeredWaveIndex = _wave.waveIndex;
            _spawner.SpawnMidBoss(_wave.midBossId);
        }
    }
}
