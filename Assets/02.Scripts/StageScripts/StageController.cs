using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField] private StageResultHandler _resultHandler;
    [SerializeField] private PlayerRewardHandler _playerRewardHandler;

    [SerializeField] private StageTimerTextUI _timerUI;

    public PlayerRewardHandler RewardHandler => _playerRewardHandler;

    private StageEvents _events;
    private StageTimer _timer;
    private StageStateMachine _stateMachine;

    private WaveTimeline _timeline;
    private WaveScheduler _waveScheduler;

    private IMonsterSpawnSystem _spawner;
    private IMonsterRegistry _registry;

    private StagePlan _plan;

    private bool _isPaused;
    private bool _ended;

    private void Awake()
    {
        _events = new StageEvents();
        _timer = new StageTimer(_events);
        _stateMachine = new StageStateMachine();
    }

    private void OnDestroy()
    {
        if (_events != null)
        {
            _events.OnMainTimerReachedLimit -= HandleMainTimerReachedLimit;
            _events.OnBossTimerExpired -= HandleBossTimerExpired;

            _events.OnStageCleared -= HandleStageClearedSignal;
            _events.OnStageFailed -= HandleStageFailedSignal;
        }

        if (_registry is MonsterRegistry mr)
        {
            mr.OnUnregistered -= HandleMonsterUnregistered;
        }

        if (_spawner is RuleBasedMonsterSpawner rb)
        {
            _events.OnMainSecondTick -= rb.SetMainSeconds;
        }
    }

    private void Update()
    {
        // 타이머 일시정지(컷신/팝업/백그라운드 등)
        _timer.SetPaused(_isPaused);

        if (_ended) return;
        if (_isPaused) return;

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

        _ended = false;
        _isPaused = false;

        _timer.Configure(_plan.mainLimitSeconds, _plan.bossLimitSeconds);

        Time.timeScale = 1;

        _timeline = new WaveTimeline(_plan.wavePlans);
        _waveScheduler = new WaveScheduler(_events, _timeline, _spawner);

        // 타이머/보스 타임아웃 트리거
        _events.OnMainTimerReachedLimit -= HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired -= HandleBossTimerExpired;
        _events.OnMainTimerReachedLimit += HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired += HandleBossTimerExpired;

        //  종료 신호를 EndStage로 수렴
        _events.OnStageCleared -= HandleStageClearedSignal;
        _events.OnStageFailed -= HandleStageFailedSignal;
        _events.OnStageCleared += HandleStageClearedSignal;
        _events.OnStageFailed += HandleStageFailedSignal;

        // Registry 이벤트(보스 죽음 감지)
        if (_registry is MonsterRegistry mr)
        {
            mr.OnUnregistered -= HandleMonsterUnregistered;
            mr.OnUnregistered += HandleMonsterUnregistered;
        }

        if (spawner is RuleBasedMonsterSpawner rb)
        {
            rb.SetStageModifiers(plan.stageHpModifier, plan.stagePowerModifier,
        plan.towerHpModifier, plan.towerPowerModifier);

            _events.OnMainSecondTick -= rb.SetMainSeconds;
            _events.OnMainSecondTick += rb.SetMainSeconds;

            rb.ApplySpecialGroup(new List<SpecialMonsterRow>(plan.specialRows ?? new List<SpecialMonsterRow>()));
        }

        if (_resultHandler != null)
        {
            // Handler는 UI/입력만 하도록 바뀔 예정이라 controller도 넘기는게 좋음
            _resultHandler.Bind(_events, this);
        }

        if (_timerUI != null)
            _timerUI.Bind(_events);

        _stateMachine.ChangeState(new CombatState(_timer, _waveScheduler));
        _spawner.SetSpawningEnabled(true);
    }

    public void SetPaused(bool isPaused)
    {
        this._isPaused = isPaused;
    }

    private void HandleMainTimerReachedLimit()
    {
        Debug.Log("[Stage] Main timer reached limit -> BossState");
        // 메인타이머 15분 도달 → 보스 상태로 전환
        Debug.Log($"[Stage] Main timer reached. planNull={_plan == null}, bossId={(_plan != null ? _plan.bossId : -999)}");
        Debug.Log($"[Stage] spawnerNull={_spawner == null}, registryNull={_registry == null}, timerNull={_timer == null}");

        IStageState _bossState = new BossState(_timer, _registry, _spawner, _plan.bossId);
        _stateMachine.ChangeState(_bossState);
    }

    private void HandleBossTimerExpired()
    {
        Debug.Log("[Stage] Boss timer expired -> FAIL");
        _events.RaiseStageFailed(StageFailReason.BossTimeout);
    }

    // 보스 사망 판정
    private void HandleMonsterUnregistered(GameObject obj)
    {
        if (_ended) return;

        if (!(_registry is MonsterRegistry mr)) return;

        if (mr.IsBoss(obj))
        {
            Debug.Log("[Stage] Boss destroyed -> CLEAR");
            _events.RaiseStageCleared();
        }
    }

    private void HandleStageClearedSignal()
    {
        if (_ended) return;
        EndStage(StageResult.Clear, StageFailReason.기타);
    }

    private void HandleStageFailedSignal(StageFailReason reason)
    {
        if (_ended) return;
        EndStage(StageResult.Fail, reason);
    }

    public void EndStage(StageResult result, StageFailReason reason)
    {
        if (_ended) return;
        _ended = true;

        Time.timeScale = 0;

        // 스폰 정지
        _spawner?.SetSpawningEnabled(false);

        if (_registry is MonsterRegistry mr)
        {
            mr.OnUnregistered -= HandleMonsterUnregistered;
            mr.MarkBoss(null); // 보스 마킹 제거 (null 허용)
        }

        // 몬스터 정리
        _registry?.KillAllMonsters();

        // 타이머 정지
        _timer?.StopMain();
        _timer?.StopBoss();

        // 결과 상태로 전환 (보상/플로우는 상태에서 처리)
        if (result == StageResult.Clear)
            _stateMachine.ChangeState(new SuccessState(this));
        else
            _stateMachine.ChangeState(new FailState(this, reason));

        // UI/로그용 단일 이벤트
        _events.RaiseStageResult(result, reason);
    }

}
