using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable, ISkillable
{
    [SerializeField] public float _maxhp = 15;
    [SerializeField] public float _attackPower = 10;
    [SerializeField] public float _defensePower = 1;
    [SerializeField] public float _evasionChance = 10;                    // 회피율
    [SerializeField] public float _criticalChance = 10;
    [SerializeField] public float _criticalDamageMultiplier = 2;          // 치명타 피해 배율
    [SerializeField] public float _criticalResistance = 10;               // 치명 저항
    [SerializeField] public float _moveSpeed = 5;
    [SerializeField] public float _healthRegen = 10;                      // Hp 회복량
    [SerializeField] public float _outGameMaxExp = 50;
    [SerializeField] public int _outGameLevel = 1;

    [Header("인게임 정보")]
    [SerializeField] public int _inGameLevel = 1;
    [SerializeField] public float _inGameMaxExp = 50;
    [SerializeField] public float _expGainRate = 10;                    // 경험치 획득량
    [SerializeField] public float _pickupRange = 1;                     // 아이템 줍는 범위
    [SerializeField] private ItemPickupSystem _itemPickupSystem;        // 아이템 줍는 범위 콜라이더

    [Header("이벤트 채널")]
    [SerializeField] private FloatEventChannelSO _onHit;
    [SerializeField] private FloatEventChannelSO _onPickupExp;
    [SerializeField] private FloatEventChannelSO _onGainInGameExp;
    [SerializeField] private IntEventChannelSO _onInGameLevelUp;
    [SerializeField] private FloatEventChannelSO _onGainOutGameExp;
    [SerializeField] private IntEventChannelSO _onOutGameLevelUp;
    [SerializeField] private VoidEventChannelSO _onDead;


    [Header("추가가 필요한 데이터")]
    [SerializeField] private float _itemDropRate = 1;
    [SerializeField] private float _goldGainRate = 1;
    

    private float _currentHp;
    private PlayerHit _hitSystem;
    private PlayerLevelSystem _levelSystem;
    private PlayerLoadout _myEnhancement;
    private PlayerStatCalculator _statCalculator;
    private PlayerStateMachine _stateMachine;


    public float Hp => Mathf.Max(_currentHp, 0);
    public float ItemDropRate => _statCalculator.GetFinalSat(_itemDropRate, RELIC_STAT.ItemDropRate);
    public float GoldGainRate => _statCalculator.GetFinalSat(_goldGainRate, RELIC_STAT.GoldGainRate);


    // 스킬 공유 데이터 (인트 수정 필요)
    public float AttackPower => _statCalculator.GetFinalSat(_attackPower, EQUIP_STAT.AttackPower);
    public float DefensePower => _statCalculator.GetFinalSat(_defensePower, EQUIP_STAT.DefensePower);
    public float MoveSpeed => _statCalculator.GetFinalSat(_moveSpeed, EQUIP_STAT.MoveSpeed);
    public float CriticalChance => _statCalculator.GetFinalSat(_criticalChance, EQUIP_STAT.CriticalChance);
    public float CriticalDamageMultiplier => _criticalDamageMultiplier;
    public float EvasionChance => _statCalculator.GetFinalSat(_evasionChance, EQUIP_STAT.EvasionChance);
    public float ExpGainRate => _statCalculator.GetFinalSat(_expGainRate, RELIC_STAT.ExpGainRate);
    public float PickupRange => _pickupRange;
    public Vector2 MoveDir => _stateMachine.MoveDir;
    public Vector2 AttackDir { get; private set; }
    public Vector2 LastAttackDir { get; private set; }


    //
    // 아웃 게임 레벨업시, 증가하는 스탯을 위해 붙여둠
    public LevelupTestStat TestLevelStat;
    //



    private void Awake()
    {
        //
        if(TestLevelStat == null)
            TestLevelStat = GetComponent<LevelupTestStat>();
        //

        _currentHp = _maxhp;
        _hitSystem = new PlayerHit(this);
        _levelSystem = new PlayerLevelSystem(this);
        _myEnhancement = ItemManager.Instance.Loadout;
        _statCalculator = new PlayerStatCalculator(_myEnhancement);

        _stateMachine = new PlayerStateMachine(this);
    }

    private void OnEnable()
    {
        _itemPickupSystem.OnEnterPickupRange += OnItemPickup;
        _onPickupExp.AddListener(GainInGameExp);
    }

    private void OnDisable()
    {
        _itemPickupSystem.OnEnterPickupRange -= OnItemPickup;
        _onPickupExp.RemoveListener(GainInGameExp);
    }

    private void Start()
    {
        _itemPickupSystem.SetColliderRange(_pickupRange);
        AttackDir = new Vector2(0, -1);
    }

    private void Update()
    {
        _stateMachine.Update();
    }



    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx) => 
        _stateMachine.MoveInput(ctx.ReadValue<Vector2>().normalized);


    // 조이스틱 방향으로 공격
    public void StickAttackDir(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            AttackDir = ctx.ReadValue<Vector2>().normalized;

        else if (ctx.canceled)
            AttackDir = Vector2.zero;

        if (AttackDir != Vector2.zero)
            LastAttackDir = AttackDir;
    }
    // 마우스 방향으로 공격
    public void MouseAttackDir(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        AttackDir = (mousePos - (Vector2)transform.position).normalized;
    }
    

    // 피격시, 발동되는 메서드
    public void TakeDamage(float damage)
    {
        float hp = _hitSystem.OnHit(_currentHp, damage);

        if(_currentHp != hp)
        {
            _currentHp = hp;
            _onHit.OnStartEvent(Hp);
        }

        if (Hp <= 0)
            OnDead();
    }
    private void OnDead()
    {
        _onDead.OnStartEvent();
        Destroy(gameObject);
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

    //
    [ContextMenu("스텟출력")]
    public void TestStat()
    {
        float a = 0;
        a = AttackPower;
        a = DefensePower;
        a = MoveSpeed;
        a = CriticalChance;
        a = EvasionChance;
        a = CriticalDamageMultiplier;
        a = ExpGainRate;
        a = ItemDropRate;
        a = GoldGainRate;
    }
    //
}
