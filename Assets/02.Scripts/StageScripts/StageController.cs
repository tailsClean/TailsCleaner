using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public static StageController Instance { get; private set; }

    [SerializeField] private StageResultHandler _resultHandler;
    [SerializeField] private PlayerRewardHandler _playerRewardHandler;
    [SerializeField] private StageTimerTextUI _timerUI;

    [SerializeField] private VoidEventChannelSO _onPlayerDead;
    public StagePlan CurrentPlan => _plan;
    public StageEvents Events => _events;
    public StageTimer Timer => _timer;

    private StageEvents _events;
    private StageTimer _timer;
    private StageStateMachine _stateMachine;

    private WaveTimeline _timeline;
    private WaveScheduler _waveScheduler;

    private IMonsterSpawnSystem _spawner;
    private IMonsterRegistry _registry;

    private StagePlan _plan;

    public bool IsSkillSelectOpen { get; private set; }
    public bool IsBossIntroPending { get; private set; }
    public bool IsBossIntroPlaying { get; private set; }
    public bool IsGameplayBlocked { get; private set; }

    public bool IsGameplayTemporarilyBlocked =>
        IsSkillSelectOpen || IsBossIntroPlaying || IsGameplayBlocked;

    private bool _isPaused;
    private bool _ended;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _events = new StageEvents();
        _timer = new StageTimer(_events);
        _stateMachine = new StageStateMachine();
    }

    private void Start()
    {
        _timerUI = UIManager.Instance.StageTimer;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

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

        if (_onPlayerDead != null)
            _onPlayerDead.RemoveListener(HandlePlayerDead);
    }

    private void Update()
    {
        _timer.SetPaused(_isPaused);

        if (_ended) return;
        if (_isPaused) return;

        float _deltaTime = Time.deltaTime;

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

        IsSkillSelectOpen = false;
        IsBossIntroPending = false;
        IsBossIntroPlaying = false;
        IsGameplayBlocked = false;

        _timer.Configure(_plan.mainLimitSeconds, _plan.bossLimitSeconds);

        Time.timeScale = 1f;

        _timeline = new WaveTimeline(_plan.wavePlans);
        _waveScheduler = new WaveScheduler(_events, _timeline, _spawner);

        _events.OnMainTimerReachedLimit -= HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired -= HandleBossTimerExpired;
        _events.OnMainTimerReachedLimit += HandleMainTimerReachedLimit;
        _events.OnBossTimerExpired += HandleBossTimerExpired;

        _events.OnStageCleared -= HandleStageClearedSignal;
        _events.OnStageFailed -= HandleStageFailedSignal;
        _events.OnStageCleared += HandleStageClearedSignal;
        _events.OnStageFailed += HandleStageFailedSignal;

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
            _resultHandler.Bind(_events, this);
        }

        if (_onPlayerDead != null)
        {
            _onPlayerDead.RemoveListener(HandlePlayerDead);
            _onPlayerDead.AddListener(HandlePlayerDead);
        }

        if (_timerUI == null)
        {
            _timerUI = UIManager.Instance != null ? UIManager.Instance.StageTimer : null;
        }

        if (_timerUI != null)
        {
            _timerUI.Bind(_events);
        }
        else
        {
            Debug.LogWarning("[StageController] StageTimerTextUI is null. Timer UI binding skipped.");
        }

        _stateMachine.ChangeState(new CombatState(_timer, _waveScheduler));
        _spawner.SetSpawningEnabled(true);

        if (UIManager.Instance != null && UIManager.Instance.StageWaveBanner != null)
        {
            Debug.Log("[StageController] PlayStageStart 호출");
            StartCoroutine(UIManager.Instance.StageWaveBanner.PlayStageStart());
        }
        else
        {
            Debug.Log("[StageController] StageWaveBanner가 null이라 호출 불가");
        }
    }

    public void SetPaused(bool isPaused)
    {
        _isPaused = isPaused;
    }

    private void HandleMainTimerReachedLimit()
    {
        Debug.Log("[Stage] Main timer reached limit");

        if (IsSkillSelectOpen)
        {
            Debug.Log("[Stage] SkillSelect is open -> BossIntro pending");
            IsBossIntroPending = true;
            return;
        }

        EnterBossState();
    }

    private void EnterBossState()
    {
        if (_plan == null || _spawner == null || _registry == null || _timer == null)
        {
            Debug.LogError("[Stage] Cannot enter BossState. Required refs are null.");
            return;
        }

        IStageState bossState = new BossState(this, _timer, _registry, _spawner, _plan.bossId);
        _stateMachine.ChangeState(bossState);
    }

    private void HandleBossTimerExpired()
    {
        Debug.Log("[Stage] Boss timer expired -> FAIL");
        _events.RaiseStageFailed(StageFailReason.BossTimeout);
    }

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

    private void HandlePlayerDead()
    {
        if (_ended) return;
        _events.RaiseStageFailed(StageFailReason.PlayerDead);
    }

    public void EndStage(StageResult result, StageFailReason reason)
    {
        if (_ended) return;
        _ended = true;

        Time.timeScale = 0f;

        _spawner?.SetSpawningEnabled(false);

        if (_registry is MonsterRegistry mr)
        {
            mr.OnUnregistered -= HandleMonsterUnregistered;
            mr.MarkBoss(null);
        }

        _registry?.KillAllMonsters();

        _timer?.StopMain();
        _timer?.StopBoss();

        if (result == StageResult.Clear)
        {
            _stateMachine.ChangeState(new SuccessState(this));
        }
        else if (result == StageResult.Abandon)
        {
            _stateMachine.ChangeState(new AbandonState(this));
        }
        else
        {
            _stateMachine.ChangeState(new FailState(this, reason));
        }

        _events.RaiseStageResult(result, reason);
    }

    public void SetGameplayBlocked(bool blocked)
    {
        IsGameplayBlocked = blocked;
    }

    public void SetBossIntroPlaying(bool value)
    {
        IsBossIntroPlaying = value;
    }

    public void NotifySkillSelectOpened()
    {
        IsSkillSelectOpen = true;
        SetPaused(true);

        Debug.Log("[Stage] SkillSelect opened -> game paused");
    }

    public void NotifySkillSelectClosed()
    {
        IsSkillSelectOpen = false;
        SetPaused(false);

        Debug.Log("[Stage] SkillSelect closed -> game resumed");

        if (IsBossIntroPending)
        {
            Debug.Log("[Stage] BossIntroPending detected -> enter BossState");
            IsBossIntroPending = false;
            EnterBossState();
        }
    }
}