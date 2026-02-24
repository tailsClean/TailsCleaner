using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable, ISkillable
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
    [SerializeField] private float _experienceGainRate = 10;            // 경험치 획득량
    [SerializeField] private float _attackInterval = 0.5f;              // 자동공격 주기
    [SerializeField] private float _pickupRange = 1;                    // 아이템 줍는 범위
    [SerializeField] private ItemPickup _itemPickupCollider;            // 아이템 줍는 범위 콜라이더
    [SerializeField] private Bullet _bulletPrefab;

    [Header("이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onHit;
    [SerializeField] private IntEventChannelSO _onPickupExp;
    [SerializeField] private IntEventChannelSO _onGainExp;              // 경험치 획득시 알리는 신호
    [SerializeField] private IntEventChannelSO _onLevelUp;
    [SerializeField] private VoidEventChannelSO _onDead;
    

    private int _currentHp;
    private int _metaCurrentExp;
    private int _combatCurrentExp;

    private PlayerHit _hitSystem;
    private PlayerAttack _attackSystem;
    private PlayerCombatLevelSystem _levelSystem;
    private PlayerEquipment _myEquipment;
    private PlayerStateMachine _stateMachine;


    public int Hp => Mathf.Max(_currentHp, 0);
    public Transform AttackTarget => GetTarget(_attackSystem.AttackDir);  // 조준형 스킬 사용을 위한 타겟
    public Bullet BulletPrefab => _bulletPrefab;


    // 스킬 공유 데이터
    public int AttackDamage => _attackPower;                               // 최종 데미지 수치
    public int DefensePower => _defensePower;
    public int MoveSpeed => _moveSpeed + _myEquipment.GetMoveSpeedIncrease();
    public int CriticalChance => _criticalChance;
    public int CriticalDamageMultiplier => 2;
    public int EvasionChance => _evasionChance;
    public float ExperienceGainRate => _experienceGainRate;
    public float PickupRange => _pickupRange;
    public Vector2 MoveDir => _stateMachine.MoveDir;
    public Vector2 AttackDir => _attackSystem.AttackDir;



    private void Awake()
    {
        _currentHp = _maxhp;
        _hitSystem = new PlayerHit(this);
        _attackSystem = new PlayerAttack(this, _bulletPrefab, _attackInterval);
        _levelSystem = new PlayerCombatLevelSystem(this, _combatMaxExp);
        _myEquipment = new PlayerEquipment(PlayerDataTransfer.Equipments);

        _stateMachine = new PlayerStateMachine(this);
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
        _attackSystem.SetDirection(new Vector2(0, -1));
    }

    private void Update()
    {
        _stateMachine.Update();

        _attackSystem.OnAttack();
    }



    // 이동 기능
    public void OnMove(InputAction.CallbackContext ctx) => 
        _stateMachine.MoveInput(ctx.ReadValue<Vector2>());

    // 공격 기능
    public Bullet FireBullet(Bullet bulletPrefab, Vector2 spawnPos) => 
        Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

    // 조이스틱 방향으로 공격
    public void StickAttackDir(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _attackSystem.SetDirection(ctx.ReadValue<Vector2>());
    }
    // 마우스 방향으로 공격
    public void MouseAttackDir(InputAction.CallbackContext ctx)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(ctx.ReadValue<Vector2>());
        _attackSystem.SetDirection(mousePos - (Vector2)transform.position);
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
        Destroy(gameObject);
        _onDead.OnStartEvent();
    }

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
