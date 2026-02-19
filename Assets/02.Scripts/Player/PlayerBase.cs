using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable
{
    [SerializeField] private float _Maxhp = 15;
    [SerializeField] private float _attackPower = 2;
    [SerializeField] private float _defensePower = 1;
    [SerializeField] private float _evasionChance = 10;              // 회피율
    [SerializeField] private float _criticalChance = 10;
    [SerializeField] private float _criticalResistance = 10;
    [SerializeField] private float _healthRegen = 10;
    [SerializeField] private int _metaLevel = 1;
    [SerializeField] private float _metaMaxExp = 50;

    
    [SerializeField] private int _combatLevel = 1;
    [SerializeField] private float _combatMaxExp = 50;
    [SerializeField] private float _experienceGainRate = 10;        // 경험치 획득률
    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _pickupRange = 1;
    [SerializeField] private float _attackInterval = 0.5f;          // 자동공격 주기
    [SerializeField] private ItemPickup _itemPickup;                // 아이템 줍는 범위(콜라이더)를 가짐
    [SerializeField] private GameObject _bulletPrefab;

    private float _hp;
    private float _metaCurrentExp;
    private float _combatCurrentExp;

    private SpriteRenderer _mySprite;
    private Vector2 _moveDir;
    private Vector2 _attackDir;
    private bool _isInvincible;                                     // 피격 무적상태 여부
    private float _timer;
    private Dictionary<EQUIPMENT, PlayerEquipment> _myEquipment;

    public event Action<float, float> OnGainExp;                    // 경험치 획득시 알리는 신호
    public event Action<EQUIPMENT> OnSetEquipment;                  // 장비가 바뀌었다는 것을 알리는 신호
    public event Action<float> OnUpdateUI;

    public float Hp => Mathf.Max(_hp, 0);
    public float FinalDamage => _attackPower;                       // 최종 데미지 수치
    public bool IsInvincible => _isInvincible;
    public Transform AttackTarget => GetTarget(_attackDir);         // 조준형 스킬 사용을 위한 타겟
    public Dictionary<EQUIPMENT, PlayerEquipment> MyEquipment => _myEquipment;


    private void Awake()
    {
        _mySprite = GetComponent<SpriteRenderer>();
        _myEquipment = new Dictionary<EQUIPMENT, PlayerEquipment>();
        _hp = _Maxhp;
    }

    private void OnEnable()
    {
        _itemPickup.OnEnterPickupRange += OnItemPickup;
    }

    private void OnDisable()
    {
        _itemPickup.OnEnterPickupRange -= OnItemPickup;
    }

    private void Start()
    {
        _itemPickup.SetColliderRange(_pickupRange);
        _attackDir = new Vector2(transform.localScale.x, 0);
    }

    private void Update()
    {
        transform.Translate(_moveDir * Time.deltaTime * _moveSpeed);

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
        if (_isInvincible)
            return;

        _hp -= damage;
        OnUpdateUI?.Invoke(Hp);
        Debug.Log("현재 체력: " + Hp);

        StartCoroutine(StartHitInvincibility());
    }
    // 피격 시, 무적 + 피격이펙트(깜빡임)
    private IEnumerator StartHitInvincibility()
    {
        _isInvincible = true;

        // 깜빡이는 메서드
        var wait = new WaitForSeconds(0.2f);
        Color original = _mySprite.color;
        for(int i = 0; i < 3; i++)
        {
            _mySprite.color = new Color(0, 0, 0);
            yield return wait;

            _mySprite.color = original;
            yield return wait;
        }

        _isInvincible = false;
    }


    // 경험치 획득 로직
    public void GainExperience(float experience)
    {
        _combatCurrentExp += experience;

        if(_combatCurrentExp >= _combatMaxExp)
        {
            _combatCurrentExp -= _combatMaxExp;
            _combatLevel++;
        }

        OnGainExp?.Invoke(_combatLevel, _combatCurrentExp);
    }
    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item)
    {
        Vector2 itemPos = item.MyTransform.position;
        Vector2 myPos = transform.position;
        
        // 마지막 인자갑은 이동 속도
        item.MyTransform.position = Vector2.MoveTowards(itemPos, myPos, 1f * Time.deltaTime);
    }


    // 장비를 교체하는 메서드
    public void SetEquipment(PlayerEquipment equipment)
    {
        if(!_myEquipment.TryAdd(equipment.EquipmentPart, equipment))
            _myEquipment[equipment.EquipmentPart] = equipment;

        OnSetEquipment?.Invoke(equipment.EquipmentPart);
    }


    // 조준형 스킬을 위한 타겟 검사
    private Transform GetTarget(Vector2 dir)
    {
        Vector2 origine = (Vector2)transform.position + dir;
        var hit = Physics2D.Raycast(origine, dir);
        Debug.Log(hit.collider.gameObject.name);

        return hit.transform;
    }

    public enum EQUIPMENT
    {
        Hat, Cloak, Weapon, Shoes, Relic
    }
}
