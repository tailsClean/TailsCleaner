using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable, ISkillable, ISkillStat, IPlayerAni
{
    [field: SerializeField] public PlayerDataSO Data { get; private set; }

    [Header("아이템 픽업 기능을 위한 콜라이더")]
    [SerializeField] private ItemPickupSystem _itemPickupSystem;        // 아이템 줍는 범위 콜라이더

    [Header("이벤트 채널")]
    [SerializeField] private FloatEventChannelSO _onHit;
    [SerializeField] private FloatEventChannelSO _onHeal;
    [SerializeField] private FloatEventChannelSO _onPickupExp;
    [SerializeField] private FloatEventChannelSO _onGainInGameExp;
    [SerializeField] private IntEventChannelSO _onInGameLevelUp;
    [SerializeField] private FloatEventChannelSO _onGainOutGameExp;
    [SerializeField] private IntEventChannelSO _onOutGameLevelUp;
    [SerializeField] private VoidEventChannelSO _onDead;

    // 플레이어 기능별 계산 클래스
    private PlayerHpSystem _hpSystem;
    private PlayerLevelSystem _levelSystem;
    private PlayerLoadout _myEnhancement;
    private PlayerStatCalculator _statCalculator;
    private PlayerStateMachine _stateMachine;
    private PlayerAni _playerAni;


    public float CurrentHp => _hpSystem.CurrentHp;
    public float InGameMaxExp => _levelSystem.InGameMaxExp;
    public float ItemDropRate => _statCalculator.GetFinalSat(Data.ItemDropRate, PLAYER_STAT.ItemDropRate);
    public float GoldGainRate => _statCalculator.GetFinalSat(Data.GoldGainRate, PLAYER_STAT.GoldGainRate);


    public float MaxHp => _hpSystem.MaxHp;
    public int MaxShield => _hpSystem.MaxSield;
    public int CurrentShield => _hpSystem.CurrentSield;
    public float AttackPower => _statCalculator.GetFinalSat(Data.AttackPower, PLAYER_STAT.AttackPower);
    public float DefensePower => _statCalculator.GetFinalSat(Data.DefensePower, PLAYER_STAT.DefensePower);
    public float MoveSpeed => _statCalculator.GetFinalSat(Data.MoveSpeed, PLAYER_STAT.MoveSpeed);
    public float CriticalChance => _statCalculator.GetFinalSat(Data.CriticalChance, PLAYER_STAT.CriticalChance);
    public float CriticalDamageMultiplier => Data.CriticalDamageMultiplier;
    public float EvasionChance => _statCalculator.GetFinalSat(Data.EvasionChance, PLAYER_STAT.EvasionChance);
    public float ExpGainRate => _statCalculator.GetFinalSat(Data.ExpGainRate, PLAYER_STAT.ExpGainRate);
    public float PickupRange => Data.PickupRange;
    public Vector2 MoveDir => _stateMachine.MoveDir;
    public Vector2 AttackDir { get; private set; }
    public Vector2 LastAttackDir { get; private set; }
    public float AttackSpeed  => 100f;


    private void Awake()
    {
        Data.Init(PlayerDataSO.PlayerID);
        _levelSystem = new PlayerLevelSystem(this);
        _myEnhancement = ItemManager.Instance.Loadout;
        _statCalculator = new PlayerStatCalculator(_myEnhancement, _levelSystem);
        _playerAni = new PlayerAni(GetComponent<Animator>());
        _hpSystem = new PlayerHpSystem(this, _statCalculator);

        _stateMachine = new PlayerStateMachine(this);
    }

    private void OnEnable()
    {
        EventConnect();
    }

    private void OnDisable()
    {
        EventDisconnect();
    }

    private void Start()
    {
        _hpSystem.Init(MaxHp);
        _itemPickupSystem.SetColliderRange(Data.PickupRange);
        AttackDir = new Vector2(0, -1);
    }

    private void Update()
    {
        _stateMachine.Update();
    }



    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx)
    {
        _stateMachine.MoveInput(ctx.ReadValue<Vector2>().normalized);

        if(_stateMachine.MoveDir == Vector2.zero)
            PlayAni(PlayerAni.Idle);
        else
            PlayAni(PlayerAni.Move);
    }


    //// 조이스틱 방향으로 공격
    //public void StickAttackDir(InputAction.CallbackContext ctx)
    //{
    //    if (ctx.performed)
    //        AttackDir = ctx.ReadValue<Vector2>().normalized;

    //    else if (ctx.canceled)
    //        AttackDir = Vector2.zero;

    //    if (AttackDir != Vector2.zero)
    //        LastAttackDir = AttackDir;
    //}
    //// 마우스 방향으로 공격
    //public void MouseAttackDir(InputAction.CallbackContext ctx)
    //{
    //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
    //    AttackDir = (mousePos - (Vector2)transform.position).normalized;
    //}


    public void Heal(float amount)
    {
        _hpSystem.OnHeal(amount);
        _onHeal.OnStartEvent(CurrentHp);
    }

    // 피격시, 발동되는 메서드
    public void TakeDamage(float damage)
    {
        _hpSystem.Hit(damage);
        _onHit.OnStartEvent(CurrentHp);

        if (_hpSystem.IsDead)
            OnDead();
    }
    private void OnDead()
    {
        _onDead.OnStartEvent();
        _playerAni.PlayAni(PlayerAni.Dead);
    }

    // 최대 실드량 갱신
    public void SetMaxShield(int maxShield) => _hpSystem.SetMaxShield(maxShield);
    // 현재 실드량 추가
    public void AddShield(int count)
    {
        if(count < 0)
        { Debug.LogWarning("실드 추가량이 음수입니다."); return; }

        _hpSystem.AddShield(count);
    }



    // 인게임 경험치 획득 로직
    public void GainInGameExp(float exp)
    {
        bool isLevelUp = _levelSystem.GainExp(PlayerLevelSystem.GameMode.InGame, exp);
        _onGainInGameExp.OnStartEvent(_levelSystem.InGameCurrentExp);

        if (isLevelUp)
            _onInGameLevelUp.OnStartEvent(_levelSystem.InGameLevel);
    }
    // 아웃게임 경험치 획득 로직
    public void GainOutGameExp(float exp)
    {
        bool isLevelUp = _levelSystem.GainExp(PlayerLevelSystem.GameMode.OutGame, exp);
        _onGainOutGameExp.OnStartEvent(_levelSystem.OutGameCurrentExp);

        if (isLevelUp)
            _onOutGameLevelUp.OnStartEvent(_levelSystem.OutGameLevel);
    }


    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item) => _itemPickupSystem.ItemPickup(transform, item);


    // 스킬 스탯값 세팅
    public void SetSkillStat(PlayerStatFlat flat, PlayerStatMul multi) =>
        _statCalculator.SetSkillStat(flat, multi);

    // 애니메이션 재생
    public void PlayAni(string aniName) => _playerAni.PlayAni(aniName);


    // 이벤트 연결
    private void EventConnect()
    {
        _hpSystem.OnHit += () => PlayAni(PlayerAni.Hit);
        _itemPickupSystem.OnEnterPickupRange += OnItemPickup;
        _onPickupExp.AddListener(GainInGameExp);
    }

    private void EventDisconnect()
    {
        _hpSystem.OnHit -= () => PlayAni(PlayerAni.Hit);
        _itemPickupSystem.OnEnterPickupRange -= OnItemPickup;
        _onPickupExp.RemoveListener(GainInGameExp);
    }


    [ContextMenu("데미지테스트")]
    public void DamageTest()
    {
        Debug.Log(MaxHp);
        Debug.Log(CurrentHp);
        TakeDamage(1);
    }

    [ContextMenu("힐")]
    public void HealTest()
    {
        Debug.Log(MaxHp);
        Debug.Log(CurrentHp);
        Heal(10);
    }




    // 디버그용
    [ContextMenu("스탯출력")]
    public void Stat()
    {
        Debug.Log(AttackPower);
    }
    // 디버그용
}
