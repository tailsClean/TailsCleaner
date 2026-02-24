using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxhp = 15;
    [SerializeField] private int _attackPower = 10;
    [SerializeField] private int _defensePower = 1;
    [SerializeField] private int _evasionChance = 10;                   // 회피율
    [SerializeField] private int _criticalChance = 10;
    [SerializeField] private int _criticalResistance = 10;              // 치명 저항
    [SerializeField] private int _moveSpeed = 5;
    [SerializeField] private int _healthRegen = 10;                     // Hp 회복량
    [SerializeField] private int _metaLevel = 1;
    [SerializeField] private int _metaMaxExp = 50;

    [Header("인게임 정보")]
    [SerializeField] private int _combatLevel = 1;
    [SerializeField] private int _combatMaxExp = 50;
    [SerializeField] private float _experienceGainRate = 10;            // 경험치 획득률
    [SerializeField] private int _pickupRange = 1;
    [SerializeField] private float _attackInterval = 0.5f;              // 자동공격 주기
    [SerializeField] private ItemPickup _itemPickupCollider;            // 아이템 줍는 범위(콜라이더)를 가짐
    [SerializeField] private Bullet _bulletPrefab;

    [Header("이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onHit;
    [SerializeField] private IntEventChannelSO _onPickupExp;
    [SerializeField] private IntEventChannelSO _onGainExp;              // 경험치 획득시 알리는 신호
    [SerializeField] private IntEventChannelSO _onLevelUp;
    [SerializeField] private VoidEventChannelSO _onDead;
    //[SerializeField] private EquipmentEventChannelSO _onSetEquipment;   // 장비가 바뀌었다는 것을 알리는 신호
    public event Action<Equipment.PARTS> OnSetEquipment;            
    

    private int _currentHp;
    private int _metaCurrentExp;
    private int _combatCurrentExp;

    private Vector2 _moveDir;
    private Vector2 _attackDir;
    private PlayerHit _hitSystem;
    private PlayerAttack _attackSystem;
    private PlayerCombatLevelSystem _levelSystem;
    private PlayerEquipment _myEquipment;
    private PlayerStateMachine _stateMachine;


    public int Hp => Mathf.Max(_currentHp, 0);
    public int FinalDamage => _attackPower;                             // 최종 데미지 수치
    public int FinalMoveSpeed => _moveSpeed + _myEquipment.GetMoveSpeedIncrease();
    public Transform AttackTarget => GetTarget(AttackDir);              // 조준형 스킬 사용을 위한 타겟
    public Bullet BulletPrefab => _bulletPrefab;
    public Vector2 AttackDir { get; private set; }

    private void Awake()
    {
        _currentHp = _maxhp;
        _hitSystem = new PlayerHit(this);
        _attackSystem = new PlayerAttack(this, _bulletPrefab, _attackInterval);
        _levelSystem = new PlayerCombatLevelSystem(this, _combatMaxExp);
        _stateMachine = new PlayerStateMachine(this);
        _myEquipment = new PlayerEquipment(PlayerDataTransfer.Equipments);
    }

    private void OnEnable()
    {
        _itemPickupCollider.OnEnterPickupRange += OnItemPickup;
        _onPickupExp.AddListener(GainExperience);
    }

    private void OnDisable()
    {
        _itemPickupCollider.OnEnterPickupRange -= OnItemPickup;
        _onPickupExp.RemoveListener(GainExperience);
    }

    private void Start()
    {
        _itemPickupCollider.SetColliderRange(_pickupRange);
        AttackDir = new Vector2(0, -1);

    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * FinalMoveSpeed);

        _attackSystem.OnAttack();
    }



    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();

        _moveDir = dir.normalized;
        if(_moveDir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if(_moveDir.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }


    public Bullet FireBullet(Bullet bulletPrefab, Vector2 spawnPos) => Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

    // 조이스틱 방향으로 공격
    public void StickAttackDir(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _attackDir = ctx.ReadValue<Vector2>();
        AttackDir = ctx.ReadValue<Vector2>();
    }
    // 마우스 방향으로 공격
    public void MouseAttackDir(InputAction.CallbackContext ctx)
    {
        _attackDir = ctx.ReadValue<Vector2>();
        _attackDir = Camera.main.ScreenToWorldPoint(_attackDir) - transform.position;
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
        {
            OnDead();
            _onDead.OnStartEvent();
        }
    }

    private void OnDead() => Destroy(gameObject);

    // 경험치 획득 로직
    public void GainExperience(int experience)
    {
        _combatCurrentExp = _levelSystem.GainExperience(_combatCurrentExp, experience);

        if (_levelSystem.IsLevelUp)
        {
            _combatLevel += _levelSystem.LevelUpDelta.CombatLevel;
            _onLevelUp.OnStartEvent(_combatLevel);
        }

        _onGainExp.OnStartEvent(_combatCurrentExp);
    }
   
    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item) => _levelSystem.ItemPickup(transform, item);




    // 조준형 스킬을 위한 타겟 검사
    private Transform GetTarget(Vector2 dir)
    {
        Vector2 origine = (Vector2)transform.position + dir;
        var hit = Physics2D.Raycast(origine, dir);
        Debug.Log(hit.collider.gameObject.name);

        return hit.transform;
    }
}
