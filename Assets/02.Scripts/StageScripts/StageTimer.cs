using UnityEngine;

public class StageTimer
{
    private const int INVALID_SECONDS = -1;

    private readonly StageEvents _events;

    // 메인 타이머 (증가)
    private float _mainTimeSeconds;

    // 보스 타이머 (감소)
    private float _bossTimeSecondsLeft;

    // UI 갱신용 캐싱
    private int _lastMainUiSeconds;
    private int _lastBossUiSeconds;

    private bool _isMainRunning;
    private bool _isBossRunning;
    private bool _isPaused;

    private int _mainLimitSeconds; // 900초
    private int _bossLimitSeconds; // 180초

    public StageTimer(StageEvents _events)
    {
        this._events = _events;

        _lastMainUiSeconds = INVALID_SECONDS;
        _lastBossUiSeconds = INVALID_SECONDS;
    }

    // 타이머 제한 시간 설정 -> 타이머 외부에서 시간 지정할 수 있도록 제작
    public void Configure(int _mainLimitSeconds, int _bossLimitSeconds)
    {
        this._mainLimitSeconds = _mainLimitSeconds;
        this._bossLimitSeconds = _bossLimitSeconds;
    }

    // 메인 타이머 초기화 (스테이지 시작 시 호출)
    public void ResetMain()
    {
        _mainTimeSeconds = 0f;
        _lastMainUiSeconds = INVALID_SECONDS;
    }

    // 메인 타이머 스타트 및 스톱
    public void StartMain() => _isMainRunning = true;
    public void StopMain() => _isMainRunning = false;

    // 보스 타이머 시작
    // 15분 도달 후 호출됨
    public void StartBoss()
    {
        _bossTimeSecondsLeft = _bossLimitSeconds;
        _lastBossUiSeconds = INVALID_SECONDS;
        _isBossRunning = true;
    }

    public void StopBoss() => _isBossRunning = false;

    // 컷신 / 팝업 / 백그라운드 대응용 일시정지
    public void SetPaused(bool _isPaused) => _isPaused = _isPaused;

    public int GetMainTimeSecondsInt() => Mathf.FloorToInt(_mainTimeSeconds);
    public int GetBossTimeSecondsLeftInt() => Mathf.CeilToInt(_bossTimeSecondsLeft);

    // StageController.Update 에서 호출
    // 시간을 진행시키는 외부에서 실질적으로 타이머를 연결시키는 것
    public void Tick(float _deltaTime)
    {
        if (_isPaused)
            return;

        if (_isMainRunning)
            TickMain(_deltaTime);

        if (_isBossRunning)
            TickBoss(_deltaTime);
    }

    // 메인 타이머 증가 처리
    private void TickMain(float _deltaTime)
    {
        _mainTimeSeconds += _deltaTime;

        // UI는 초 단위로만 갱신
        int _uiSeconds = Mathf.FloorToInt(_mainTimeSeconds);
        if (_uiSeconds != _lastMainUiSeconds)
        {
            _lastMainUiSeconds = _uiSeconds;
            _events.RaiseMainSecondTick(_uiSeconds);
        }

        // 15분 도달 시 단 한 번만 이벤트 발생 -> _isMainRunning을 비활성화 후 리미트 이벤트 
        if (_mainTimeSeconds >= _mainLimitSeconds)
        {
            _mainTimeSeconds = _mainLimitSeconds;
            _isMainRunning = false;
            _events.RaiseMainTimerReachedLimit();
        }
    }

    // 보스 타이머 감소 처리
    private void TickBoss(float _deltaTime)
    {
        _bossTimeSecondsLeft -= _deltaTime;

        if (_bossTimeSecondsLeft < 0f)
            _bossTimeSecondsLeft = 0f;

        int _uiSecondsLeft = Mathf.CeilToInt(_bossTimeSecondsLeft);
        if (_uiSecondsLeft != _lastBossUiSeconds)
        {
            _lastBossUiSeconds = _uiSecondsLeft;
            _events.RaiseBossSecondTick(_uiSecondsLeft);
        }

        // 0초 도달 → 즉시 패배 처리 트리거
        if (_bossTimeSecondsLeft <= 0f)
        {
            _isBossRunning = false;
            _events.RaiseBossTimerExpired();
        }

    }
}
