using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxhp = 15;
    [SerializeField] private int _attackPower = 2;
    [SerializeField] private int _defensePower = 1;
    [SerializeField] private int _evasionChance = 10;               // 회피율
    [SerializeField] private int _criticalChance = 10;
    [SerializeField] private int _criticalResistance = 10;          // 치명 저항
    [SerializeField] private int _moveSpeed = 5;
    [SerializeField] private int _healthRegen = 10;                 // Hp 회복량
    [SerializeField] private int _metaLevel = 1;
    [SerializeField] private int _metaMaxExp = 50;

    [Header("인게임 정보")]
    [SerializeField] private int _combatLevel = 1;
    [SerializeField] private int _combatMaxExp = 50;
    [SerializeField] private float _experienceGainRate = 10;        // 경험치 획득률
    [SerializeField] private int _pickupRange = 1;
    [SerializeField] private float _attackInterval = 0.5f;          // 자동공격 주기
    [SerializeField] private ItemPickup _itemPickupCollider;        // 아이템 줍는 범위(콜라이더)를 가짐
    [SerializeField] private GameObject _bulletPrefab;

    [Header("이벤트 채널")]
    [SerializeField] private IntEventChannelSO _onPickupExp;
    

    private int _currentHp;
    private int _metaCurrentExp;
    private int _combatCurrentExp;

    private Vector2 _moveDir;
    private Vector2 _attackDir;
    private float _timer;
    private PlayerHit _hitSystem;
    private PlayerLevelSystem _levelSystem;
    private PlayerEquipment _myEquipment;

    public event Action<float, float> OnGainExp;                    // 경험치 획득시 알리는 신호
    public event Action<Equipment.PARTS> OnSetEquipment;            // 장비가 바뀌었다는 것을 알리는 신호
    public event Action<float> OnUpdateUI;

    public int Hp => (int)Mathf.Max(_currentHp, 0);
    public int FinalDamage => _attackPower;                         // 최종 데미지 수치
    public int FinalMoveSpeed => _moveSpeed + _myEquipment.GetMoveSpeedIncrease();
    public Transform AttackTarget => GetTarget(_attackDir);         // 조준형 스킬 사용을 위한 타겟


    private void Awake()
    {
        _myEquipment = new PlayerEquipment(PlayerDataTransfer.Equipments);
        _hitSystem = new PlayerHit(this);
        _levelSystem = new PlayerLevelSystem(this, _combatMaxExp);
        _currentHp = _maxhp;
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
        _attackDir = new Vector2(transform.localScale.x, 0);
    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * FinalMoveSpeed);

        _timer += Time.deltaTime;

        if(_timer > _attackInterval)
        {
            OnAttack();
            _timer -= _attackInterval;
        }
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


    // 공격 기능
    private void OnAttack()
    {
        if (!_bulletPrefab)
            return;

        Vector2 spawnPos = (Vector2)transform.position + _attackDir.normalized;
        var obj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);

         if (obj.TryGetComponent<IBullet>(out var bullet))
            bullet.SetDirection(_attackDir.normalized);
    }
    // 조이스틱 방향으로 공격
    public void StickAttackDir(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _attackDir = ctx.ReadValue<Vector2>();
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
        int hp = _hitSystem.TakeDamage((int)_currentHp, (int)damage);

        if(_currentHp != hp)
        {
            _currentHp = hp;
            OnUpdateUI?.Invoke(Hp);
        }
    }


    // 경험치 획득 로직
    public void GainExperience(int experience)
    {
        _combatCurrentExp = _levelSystem.GainExperience(_combatCurrentExp, experience);

        if (_levelSystem.IsLevelUp)
            _combatLevel += _levelSystem.LevelUpDelta.CombatLevel;

        OnGainExp?.Invoke(_combatLevel, _combatCurrentExp);
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
