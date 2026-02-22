using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
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
    private Dictionary<Equipment.PARTS, Equipment> _myEquipment;
    private StatCalculator _calculator;                             // 스텟 계산기

    public event Action<float, float> OnGainExp;                    // 경험치 획득시 알리는 신호
    public event Action<Equipment.PARTS> OnSetEquipment;            // 장비가 바뀌었다는 것을 알리는 신호
    public event Action<float> OnUpdateUI;

    public int Hp => (int)Mathf.Max(_hp, 0);
    public int FinalDamage => _attackPower;                         // 최종 데미지 수치
    public int FinalMoveSpeed => _calculator.GetMoveSpeed(_moveSpeed, _myEquipment);
    public Transform AttackTarget => GetTarget(_attackDir);         // 조준형 스킬 사용을 위한 타겟


    private void Awake()
    {
        _myEquipment = PlayerDataTransfer.Equipments;
        _mySprite = GetComponent<SpriteRenderer>();
        _calculator = new StatCalculator();
        _hp = _maxhp;
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
    public void GainExperience(int experience)
    {
        _combatCurrentExp += experience;

        if(_combatCurrentExp >= _combatMaxExp)
        {
            Debug.Log("레벨업으로 스텟증가(수치 입력해야 함");
        }

        OnGainExp?.Invoke(_combatLevel, _combatCurrentExp);
    }
    //레벨업 시, 스텟변화량을 받아서 증가
    private void LevelUp(StatDelta statDelta)
    {
        _combatCurrentExp -= _combatMaxExp;
        _combatLevel++;

        _maxhp += statDelta.MaxHp;
        _attackPower += statDelta.AttackPower;
        _defensePower += statDelta.DefensePower;
        _healthRegen += statDelta.HealthRegen;
        _combatLevel += statDelta.CombatLevel;
        _combatMaxExp += statDelta.CombatMaxExp;
    }
    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item)
    {
        Vector2 itemPos = item.MyTransform.position;
        Vector2 myPos = transform.position;
        
        // 마지막 인자갑은 끌어당기는 속도
        item.MyTransform.position = Vector2.MoveTowards(itemPos, myPos, 1f * Time.deltaTime);
    }


    // 조준형 스킬을 위한 타겟 검사
    private Transform GetTarget(Vector2 dir)
    {
        Vector2 origine = (Vector2)transform.position + dir;
        var hit = Physics2D.Raycast(origine, dir);
        Debug.Log(hit.collider.gameObject.name);

        return hit.transform;
    }

    [ContextMenu("장비 반영")]
    // 플레이어 스텟에 장비스텟값 추가 메서드
    public void Init()
    {
        _myEquipment = PlayerDataTransfer.Equipments;
        foreach(var equipment in _myEquipment.Values)
        {
            if(equipment != null)
                ApplyStats(equipment);
        }
    }
    // 부위별 장비 스탯 반영 메서드
    private void ApplyStats(Equipment equipment)
    {
        _myEquipment = PlayerDataTransfer.Equipments;
        //switch (equipment)
        //{
        //    case WeaponEquipment weapon:
        //        _attackPower += weapon.AttackPowerIncrease;
        //        Debug.Log($"{equipment.gameObject.name} 반영 / 공격증: {weapon.AttackPowerIncrease}");
        //        break;
        //    case HatEquipment hat:
        //        _criticalChance += hat.CriticalChanceIncrease;
        //        Debug.Log($"{equipment.gameObject.name} 반영 / 크확증: {hat.CriticalChanceIncrease}");
        //        break;
        //    case CloakEquipment cloak:
        //        _maxhp += cloak.MaxHpIncrease;
        //        _defensePower += cloak.DefensePowerIncrease;
        //        Debug.Log($"{equipment.gameObject.name} 반영 / Hp증: {cloak.MaxHpIncrease}");
        //        Debug.Log($"{equipment.gameObject.name} 반영 / 방증: {cloak.DefensePowerIncrease}");
        //        break;
        //    case ShoesEquipment shoes:
        //        _moveSpeed += shoes.MoveSpeedIncrease;
        //        _evasionChance += shoes.EvasionChanceIncrease;
        //        Debug.Log($"{equipment.gameObject.name} 반영 / 이속증: {shoes.MoveSpeedIncrease}");
        //        Debug.Log($"{equipment.gameObject.name} 반영 / 회피증: {shoes.EvasionChanceIncrease}");
        //        break;
        //}
    }

    private T ApplyEquipment<T>(Equipment.PARTS part) where T : Equipment => 
        _myEquipment[part].ApplyEquipment<T>();


    public struct StatDelta
    {
        public int MaxHp;
        public int AttackPower;
        public int DefensePower;
        public int HealthRegen;
        public int CombatLevel;
        public int CombatMaxExp;

        public StatDelta(int maxHp, int att, int def, int healthRegen, int maxExp)
        {
            MaxHp = maxHp;
            AttackPower = att;
            DefensePower = def;
            HealthRegen = healthRegen;
            CombatLevel = 1;
            CombatMaxExp = maxExp;
        }
    }
}
