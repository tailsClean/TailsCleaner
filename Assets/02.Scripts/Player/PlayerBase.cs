using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBase : MonoBehaviour, IDamageable
{
    [SerializeField] private float _hp = 100;
    [SerializeField] private float _attackPower = 10;
    [SerializeField] private float _defensePower = 5;
    [SerializeField] private float _level = 1;
    [SerializeField] private float _currentExp;
    [SerializeField] private float _pickupRange = 2;

    [SerializeField] private float _moveSpeed = 5;
    [SerializeField] private float _attackInterval = 0.5f;      // 자동공격 주기
    [SerializeField] private ItemPickup _itemPickup;            // 아이템 줍는 범위(콜라이더)를 가짐
    [SerializeField] private GameObject _bulletPrefab;


    private Vector2 _moveDir;
    private Vector2 _attackDir;
    private SpriteRenderer _mySprite;
    private bool _isInvincible;                                 // 피격 무적상태 여부
    private float _timer;
    private Dictionary<EQUIPMENT, PlayerEquipment> _myItems;

    public float Hp => Mathf.Max(_hp, 0);
    //public float FinalDamage => _attackPower;                   // 최종 데미지 수치
    public bool IsInvincible => _isInvincible;



    private void Awake()
    {
        _mySprite = GetComponent<SpriteRenderer>();
        _myItems = new Dictionary<EQUIPMENT, PlayerEquipment>();
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
        _moveDir = ctx.ReadValue<Vector2>().normalized;

        if(_moveDir.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if(_moveDir.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }


    // 공격 기능
    public void OnAttack()
    {
        if (!_bulletPrefab)
            return;

        // 마우스 위치의 벡터값
        SetAttackDir();

        Vector2 spawnPos = (Vector2)transform.position + _attackDir.normalized;
        var obj = Instantiate(_bulletPrefab, spawnPos, Quaternion.identity);

         if (obj.TryGetComponent<IBullet>(out var bullet))
            bullet.SetDirection(_attackDir.normalized);
    }
    // 공격 방향을 결정하는 메서드
    private void SetAttackDir()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        _attackDir = Camera.main.ScreenToWorldPoint(mousePos) - transform.position;
    }


    // 피격시, 발동되는 메서드
    public void TakeDamage(float damage)
    {
        if (_isInvincible)
            return;

        _hp -= damage;
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


    // 주위 아이템(경험치) 끌어모으는 메서드
    private void OnItemPickup(IPickable item)
    {
        Vector2 itemPos = item.MyTransform.position;
        Vector2 myPos = transform.position;
        
        // 마지막 파라미터는 이동 속도
        item.MyTransform.position = Vector2.MoveTowards(itemPos, myPos, 1f * Time.deltaTime);
    }

    private enum EQUIPMENT
    {
        Hat, Cloak, Weapon, Shoes, Relic
    }
}
