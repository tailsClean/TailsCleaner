using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(SkillManager))]
public class SkillStatHandler : MonoBehaviour
{
    // 플레이어 참조
    private PlayerBase _player;
    private PlayerStatCalculator _statCalculator;

    // 영구 (패시브, 장난감)
    private PlayerStatFlat _permanentFlat = new();
    private PlayerStatMul _permanentMul = new();

    // 런타임 (장판, 샤워기)
    private Dictionary<string, PlayerStatFlat> _runtimeFlats = new();
    private Dictionary<string, PlayerStatMul> _runtimeMuls = new();

    // 총합
    private PlayerStatFlat _totalFlat = new();
    private PlayerStatMul _totalMulti = new();

    // 방어막
    private int _currentShield = 0;
    private int _maxShield = 1;   // NimbleBlockModifier 획득 시 3

    // 적 강화 (저주?)
    // public EnemyBuffData EnemyBuff { get; } = new();


    private void Awake()
    {
        _player = GetComponent<PlayerBase>();
        //_statCalculator = _player.StatCalculator;
    }


    // 영구 스탯 재계산
    // SkillManager.RecheckAllModifiers() 맨 끝에서 호출
    public void RecheckPermanent()
    {
        // 초기화
        _permanentFlat.Reset();
        _permanentMul.Reset();
        //EnemyBuff.Reset();

        var skillManager = SkillManager.Instance;

        // 액티브 스킬 영구 보너스 (기차, 달, 상어, 해적선, 세탁파도 등)
        foreach (var skill in skillManager.MyActiveSkills)
            skill.ModifyPlayerBonus(_permanentFlat, _permanentMul);

        // 패시브 영구 보너스
        foreach (var passive in skillManager.MyPassiveSkills)
            passive.Modifier?.ModifyPlayerPermanent(_permanentFlat, _permanentMul, skillManager, passive.SubTag);

        //// 적 강화 수치 수집
        //foreach (var skill in skillManager.MyActiveSkills)
        //    if (skill is 적강화인터페이스 src)
        //        EnemyBuff.Add(src.EnemyStrengthBonus);


        // 방어막 최대치 갱신 (NimbleBlockModifier)
        int newMax = 1;
        foreach (var passive in skillManager.MyPassiveSkills)
        {
            if (passive.Modifier is NimbleBlockModifier nimble)
            {
                newMax = nimble.MaxShieldStack;
                break;
            }
        }

        // 방어막 최대치 설정
        SetMaxShield(newMax);

        // 설정 완료 후 주입
        Apply();
    }


    // 런타임 스탯 추가
    public void AddRuntimeFlat(string key, PlayerStatFlat flat)     // 고정
    {
        _runtimeFlats[key] = flat;
        Apply();
    }
    public void AddRunTimeMulti(string key, PlayerStatMul multi)    // 배율
    {
        _runtimeMuls[key] = multi;
        Apply();
    }

    // 런타임 스탯 제거 (flat랑 multi 같은 key 로 관리)
    public void RemoveRuntime(string key)
    {
        // 플랫 삭제 시도 후 결과
        bool changed = _runtimeFlats.Remove(key);
        // 멀티 삭제 시도 후 결과 합침
        changed |= _runtimeMuls.Remove(key);
        // 둘 중 하나라도 참이면 적용
        if (changed) Apply();
    }


    // 회복
    // 최대 체력 비율 회복
    // 초과 회복 시 청소용 비닐옷 방어막 충전 처리
    public void HealByRatio(float ratio)
    {
        //float maxHp = _player.MaxHp;
        float maxHp = 100f;                                 // 최대 체력
        float amount = maxHp * ratio;                       // 회복 비율
        float overflow = (_player.Hp + amount) - maxHp;     // 초과 회복

        // 회복
        // _player.Heal(amount);

        // 초과 회복에 비닐옷 있으면 방어막
        if (overflow > 0f && HasPassive<CleanerVinylSuitModifier>())
            TryAddShield(1);
    }

    // 최대 체력 비율 피해 (탈수 등)
    public void TakeDamageByRatio(float ratio)
        => _player.TakeDamage(/*_player.MaxHp*/ 100f * ratio);


    // 최대 방어막 설정
    public void SetMaxShield(int max)
    {
        // 최대 방어막 갱신
        _maxShield = max;
        // 현재 방어막이 최대 방어막 초과하지 않게 제한
        _currentShield = Mathf.Clamp(_currentShield, 0, _maxShield);
        // 플레이어에게 적용
        //_player.SetShield(_currentShield, _maxShield);
    }

    // 방어막 추가
    public void TryAddShield(int count)
    {
        // 현재 방어막
        int prev = _currentShield;

        // 최대 방어막 초과하지 않게
        _currentShield = Mathf.Min(_currentShield + count, _maxShield);

        // 방어막이 달라지면 갱신
        //if (_currentShield != prev)
        //    _player.SetShield(_currentShield, _maxShield);
    }

    // 탄환 제거 시 호출
    public void OnBulletCleared()
    {
        // 패시브 있으면 방어막 추가 시도
        if (HasPassive<NimbleBlockModifier>())
            TryAddShield(1);
    }

    // 피해 받을 시 방어막 감소
    // true 반환 시 피해 무효화
    public bool ConsumeShield()
    {
        if (_currentShield <= 0) return false;
        _currentShield--;
        //_player.SetShield(_currentShield, _maxShield);
        return true;
    }


    // 지속 + 런타임 합산해서 StatCalculator 주입
    // 버퍼 재사용
    private void Apply()
    {
        // 초기화
        _totalFlat.Reset();
        _totalMulti.Reset();

        // 총합에 영구 스탯 추가
        _totalFlat.Add(_permanentFlat);
        _totalMulti.Multiply(_permanentMul);

        // 총합에 런타임 스탯 추가
        foreach (var flat in _runtimeFlats.Values) _totalFlat.Add(flat);
        foreach (var multi in _runtimeMuls.Values) _totalMulti.Multiply(multi);

        // 스킬 스탯 주입
        // _statCalculator.SetSkillStat(_totalFlat, _totalMulti);

        // PlayerStatCalculator 내부에 아래처럼 만들어두고 사용하면 될듯?
        //
        // private PlayerStatFlat _skillFlat = new();
        // private PlayerStatMul _skillMulti = new();
        //
        // public void SetSkillStat(PlayerStatFlat flat, PlayerStatMul multi)
        // {
        //     _skillFlat.CopyFrom(flat);
        //     _skillMul.CopyFrom(multi);
        // }
        //
        // _skillFlat 는 고정 수치 기본 0f
        // _skillMulti는 배율 수치 기본 1f
    }


    // 보유 패시브 중 해당 패시브 있는지
    private bool HasPassive<T>() where T : PassiveModifier
    {
        foreach (var passive in SkillManager.Instance.MyPassiveSkills)
        {
            if (passive.Modifier is T) return true;
        }

        return false;
    }
}