using UnityEngine;

public class StageController : MonoBehaviour
{
    private StageEvents _events;
    private StageTimer _timer;
    private StageStateMachine _stateMachine;

    private WaveTimeline _timeline;
    private WaveScheduler _waveScheduler;

    private IMonsterSpawnSystem _spawner;
    private IMonsterRegistry _registry;

    private StagePlan _plan;

    private bool _isPaused;

    private void Awake()
    {
        _events = new StageEvents();
        _timer = new StageTimer(_events);
        _stateMachine = new StageStateMachine();

        //이후 더 추가 할 거 있으면 추가할 예정

    }

    private void OnDestroy()
    {
        if (_events != null)
        {
            _events.OnMainTimerReachedLimit -= HandleMainTimerReachedLimit;
        }

        if(_events != null)
        {
            _events.OnBossTimerExpired -= HandleBossTimerExpired;
        }
    }


    private void Update()
    {
        // 타이머 일시정지(컷신/팝업/백그라운드 등)
        _timer.SetPaused(_isPaused);

        float _deltaTime = Time.deltaTime;

        // Tick은 반드시 “단일 지점”에서만 호출
        _timer.Tick(_deltaTime);
        _stateMachine.Tick(_deltaTime);
    }

    public void StartStage(StagePlan plan, IMonsterSpawnSystem spawner, IMonsterRegistry registry)
    {
        _plan = plan;
        _spawner = spawner;
        _registry = registry;

        // 타이머 제한 시간 설정
        _timer.Configure(_plan.mainLimitSeconds, _plan.bossLimitSeconds);

        //웨이브 검색용 타임라인
        _timeline = new WaveTimeline(_plan.wavePlans); ;

        // 웨이브 트리거/이벤트 담당 스케줄러 연걸
        _waveScheduler = new WaveScheduler(_events, _timeline, _spawner);

        // 상태 전환 트리거 구독
        _events.OnMainTimerReachedLimit -= HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired -= HandleBossTimerExpired;

        _events.OnMainTimerReachedLimit += HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired += HandleBossTimerExpired;

        // 시작 상태 설정 -> Combat
        IStageState _combatState = new CombatState(_timer, _waveScheduler);
        _stateMachine.ChangeState(_combatState);
    }

    public void SetPaused(bool isPaused)
    {
        this._isPaused = isPaused;
    }

    private void HandleMainTimerReachedLimit()
    {
        Debug.Log("[Stage] Main timer reached limit -> BossState");
        // 메인타이머 15분 도달 → 보스 상태로 전환
        IStageState _bossState = new BossState(_timer, _registry, _spawner, _plan.bossId);
        _stateMachine.ChangeState(_bossState);
    }

    private void HandleBossTimerExpired()
    {
        Debug.Log("[Stage] Boss timer expired -> FAIL");
        //보스타이머 0초 도달 했을 때 -> 아마 실패 판정
    }
}
