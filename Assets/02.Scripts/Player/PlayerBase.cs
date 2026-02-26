using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable, ISkillable
{
    [SerializeField] public int _maxhp = 15;
    [SerializeField] public int _attackPower = 10;
    [SerializeField] public int _defensePower = 1;
    [SerializeField] public int _evasionChance = 10;                   // 회피율
    [SerializeField] public int _criticalChance = 10;
    [SerializeField] public int _criticalDamageMultiplier = 2;
    [SerializeField] public int _criticalResistance = 10;              // 치명 저항
    [SerializeField] public int _moveSpeed = 5;
    [SerializeField] public int _healthRegen = 10;                     // Hp 회복량
    [SerializeField] public int _outGameLevel = 1;
    [SerializeField] public int _outGameMaxExp = 50;

    [Header("인게임 정보")]
    [SerializeField] public int _inGameLevel = 1;
    [SerializeField] public int _inGameMaxExp = 50;
    [SerializeField] public float _experienceGainRate = 10;            // 경험치 획득량
    [SerializeField] public float _attackInterval = 0.5f;              // 자동공격 주기
    [SerializeField] public float _pickupRange = 1;                    // 아이템 줍는 범위
    [SerializeField] private ItemPickup _itemPickupCollider;            // 아이템 줍는 범위 콜라이더

    [Header("이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onHit;
    [SerializeField] private IntEventChannelSO _onPickupExp;
    [SerializeField] private IntEventChannelSO _onGainInGameExp;
    [SerializeField] private IntEventChannelSO _onInGameLevelUp;
    [SerializeField] private IntEventChannelSO _onGainOutGameExp;
    [SerializeField] private IntEventChannelSO _onOutGameLevelUp;
    [SerializeField] private VoidEventChannelSO _onDead;

    [Header("공격 레이어")]
    [SerializeField] private LayerMask _monsterLayer;
    

    private int _currentHp;
    private PlayerHit _hitSystem;
    private PlayerLevelSystem _levelSystem;
    private PlayerEquipment _myEquipment;
    private PlayerStateMachine _stateMachine;


    public int Hp => Mathf.Max(_currentHp, 0);
    public Transform AttackTarget => GetTarget(AttackDir);  // 조준형 스킬 사용을 위한 타겟


    // 스킬 공유 데이터
    public int AttackDamage => _attackPower;
    public int DefensePower => _defensePower;
    public int MoveSpeed => _moveSpeed + _myEquipment.GetMoveSpeedIncrease();
    public int CriticalChance => _criticalChance;
    public int CriticalDamageMultiplier => _criticalDamageMultiplier;
    public int EvasionChance => _evasionChance;
    public float ExperienceGainRate => _experienceGainRate;
    public float PickupRange => _pickupRange;
    public Vector2 MoveDir => _stateMachine.MoveDir;
    public Vector2 AttackDir { get; private set; }


    //
    private LevelupTestStat _levelUpTestStat;
    //


    private void Awake()
    {
        //
        _levelUpTestStat = GetComponent<LevelupTestStat>();
        //

        _currentHp = _maxhp;
        _hitSystem = new PlayerHit(this);
        _levelSystem = new PlayerLevelSystem(this);
        _myEquipment = new PlayerEquipment(PlayerDataTransfer.Equipments);

        _stateMachine = new PlayerStateMachine(this);
    }

    private void OnEnable()
    {
        _itemPickupCollider.OnEnterPickupRange += OnItemPickup;
        _onPickupExp.AddListener(GainInGameExp);
    }

    private void OnDisable()
    {
        _itemPickupCollider.OnEnterPickupRange -= OnItemPickup;
        _onPickupExp.RemoveListener(GainInGameExp);
    }

    private void Start()
    {
        _itemPickupCollider.SetColliderRange(_pickupRange);
        AttackDir = new Vector2(0, -1);
    }

    private void Update()
    {
        _stateMachine.Update();
    }



    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx) => 
        _stateMachine.MoveInput(ctx.ReadValue<Vector2>().normalized);

    // 공격 기능
    public Bullet FireBullet(Bullet bulletPrefab, Vector2 spawnPos) => 
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

    // 조이스틱 방향으로 공격
    public void StickAttackDir(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        AttackDir = ctx.ReadValue<Vector2>().normalized;
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
        int hp = _hitSystem.OnHit(_currentHp, (int)damage);

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
    public void GainInGameExp(int exp)
    {
        bool isLevelUp = _levelSystem.GainExp(PlayerLevelSystem.GameMode.InGame, exp);

        _onGainInGameExp.OnStartEvent(_levelSystem.InGameCurrentExp);

        if (isLevelUp)
            _onInGameLevelUp.OnStartEvent(_levelSystem.InGameLevel);
    }
    // 아웃게임 경험치 획득 로직
    public void GainOutGameExp(int exp)
    {
        bool isLevelUp = _levelSystem.GainExp(PlayerLevelSystem.GameMode.OutGame, exp);

        _onGainOutGameExp.OnStartEvent(_levelSystem.OutGameCurrentExp);

        if (isLevelUp)
            _onOutGameLevelUp.OnStartEvent(_levelSystem.OutGameLevel);
    }


    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item) => _levelSystem.ItemPickup(transform, item);




    // 조준형 스킬을 위한 타겟 검사
    private Transform GetTarget(Vector2 dir)
    {
        Vector2 origine = (Vector2)transform.position + dir;

        var hit = Physics2D.Raycast(origine, dir, Mathf.Infinity, _monsterLayer);

        if (hit.collider != null)
            return hit.transform;

        return null;
    }
}
