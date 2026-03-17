using System;
using System.Collections;
using UnityEngine;

public class PlayerHpSystem
{
    private PlayerBase _player;
    private PlayerStatCalculator _calculator;
    private SpriteRenderer _playerSprite;
    private float _maxHp;
    private int _maxSield;
    private float _currentHp;
    private int _currentSield;

    public bool IsDead => _currentHp <= 0;
    public bool IsInvincible { get; private set; }         // 피격시, 잠시 무적(true)이 됨
    public float MaxHp => _calculator.GetFinalSat(_maxHp, PLAYER_STAT.MaxHp);
    public int MaxSield => _maxSield;
    public float CurrentHp
    {
        get {  return _currentHp; }
        private set 
        { 
            _currentHp = value < MaxHp ? Mathf.Max(0, value) : MaxHp;
        }
    }
    public int CurrentSield
    {
        get { return _currentSield; }
        private set 
        { 
            _currentSield = value < _maxSield ? Mathf.Max(0, value) : _maxSield; 
        }
    }

    public event Action OnHit;
    public event Action OnShieldBloak;

    public PlayerHpSystem(PlayerBase player, PlayerStatCalculator calculator)
    {
        _player = player;
        _calculator = calculator;
        _maxHp = player.Data.Maxhp;
        _currentHp = _maxHp;
        _playerSprite = player.GetComponent<SpriteRenderer>();
    }


    public void Hit(float damage)
    {
        Debug.Log("현재" + CurrentHp);
        if (damage < 0)
        { Debug.LogError("데미지가 음수값으로 들어왔습니다."); return; }

        if (IsInvincible)
            return;

        damage = AddShield(-damage);
        CurrentHp -= damage;

        if(damage > 0)
        {
            OnHit?.Invoke();
            _player.StartCoroutine(StartHitInvincibility(true));
        }

        else if (damage == 0)
        {
            OnShieldBloak?.Invoke();
            _player.StartCoroutine(StartHitInvincibility(false));
        }
    }
    // 피격 시, 무적 + 피격이펙트(깜빡임)
    private IEnumerator StartHitInvincibility(bool HpDown)
    {
        IsInvincible = true;

        // 깜빡이는 메서드
        var wait = new WaitForSeconds(0.2f);
        Color original = _playerSprite.color;
        for (int i = 0; i < 3; i++)
        {
            if(HpDown)
                _playerSprite.color = new Color(0, 0, 0);
            yield return wait;

            _playerSprite.color = original;
            yield return wait;
        }

        IsInvincible = false;
    }

    public void OnHeal(float amount)
    {
        if (amount < 0)
        { Debug.LogError("체력회복량이 음수입니다."); return; }
        CurrentHp += amount;
    }

    public void SetMaxShield(float amount)
    {
        if (amount < 0)
        { Debug.LogError("실드 최대량 입력이 음수입니다."); return; }
        _maxSield = (int)amount;
    }

    public float AddShield(float amount)
    {
        float result = _currentSield + amount;
        CurrentSield = (int)result;
        return result < 0 ? -result : 0;
    }

    public void Init(float maxHp)
    {
        _currentHp = MaxHp;
        Debug.Log("현재 체력" + _currentHp);
        Debug.Log("현재 체력 프로퍼티" + CurrentHp);
    }
}

