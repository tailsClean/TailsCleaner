using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public static StageController Instance { get; private set; }

    [SerializeField] private StageResultHandler _resultHandler;
    [SerializeField] private PlayerRewardHandler _playerRewardHandler;
    [SerializeField] private StageTimerTextUI _timerUI;

    [SerializeField] private VoidEventChannelSO _onPlayerDead;

    public PlayerRewardHandler RewardHandler => _playerRewardHandler;
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
        this._isPaused = isPaused;
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

    private void HandlePlayerDead()
    {
        if (_ended) return;
        _events.RaiseStageFailed(StageFailReason.PlayerDead);
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
        // Abandon 분기 추가
        if (result == StageResult.Clear)
        {
            _stateMachine.ChangeState(new SuccessState(this));
        }
        else if (result == StageResult.Abandon)
        {
            _stateMachine.ChangeState(new AbandonState(this)); // [추가]
        }
        else
        {
            _stateMachine.ChangeState(new FailState(this, reason));
        }


        // UI/로그용 단일 이벤트
        _events.RaiseStageResult(result, reason);
    }

    // 보스 연출 때 플레이어 정지 및 스킬 선택UI가 선행이 되도록 하기 위한 작업
    // 보스 연출 중 gameplay block on/off
    public void SetGameplayBlocked(bool blocked)
    {
        IsGameplayBlocked = blocked;
    }

    // BossState가 직접 private set 필드를 건드리지 않고 메서드로만 요청하게 함
    public void SetBossIntroPlaying(bool value)
    {
        IsBossIntroPlaying = value;
    }

    // 스킬 선택창 오픈 시 호출
    public void NotifySkillSelectOpened()
    {
        IsSkillSelectOpen = true;
        SetPaused(true);

        Debug.Log("[Stage] SkillSelect opened -> game paused");
    }

    // 스킬 선택창 종료 시 호출
    public void NotifySkillSelectClosed()
    {
        IsSkillSelectOpen = false;
        SetPaused(false);

        Debug.Log("[Stage] SkillSelect closed -> game resumed");

        // 스킬 선택 때문에 미뤄둔 보스 진입이 있으면 여기서 실행
        if (IsBossIntroPending)
        {
            Debug.Log("[Stage] BossIntroPending detected -> enter BossState");
            IsBossIntroPending = false;
            EnterBossState();
        }
    }
}
