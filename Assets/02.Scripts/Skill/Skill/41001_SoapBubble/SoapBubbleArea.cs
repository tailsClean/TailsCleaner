using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SoapBubbleArea : SkillArea<SoapBubbleModifierData>
{
    // 적 체류 시작 시간
    private readonly Dictionary<MonsterBase, float> _monsterEnterTimes = new();

    // 이미 기절 처리된 적 (1회 제한)
    private readonly HashSet<MonsterBase> _stunnedMonsters = new();

    public override void Init(ActiveSkill owner, SoapBubbleModifierData modifierData, Vector2 dir = default)
    {
        _monsterEnterTimes.Clear();
        _stunnedMonsters.Clear();

        base.Init(owner, modifierData, dir);
    }

    protected override void Update()
    {
        // 스턴 체류 체크 추가해야함
        // 

        // 틱 주기 처리 추가 base를 통해 수명 체크 ,스노우 볼링
        base.Update();
    }


    protected override void OnMonsterEnter(MonsterBase monster)
    {
        // 약화 상태 체크
        // 약화 상태인지 체크 후 최대 체력 감소
        //if (_runTimeFinalStat.MaxHpDecreaseRate >= 0 && monster.IsDebuffed)
        //    monster.DecreaseMaxHp(MaxHpDecreaseRate);  

        // 빨래당함 슬로우
        if (_modifierData.SlowOnArea == true)
        {
            // monster.AddSpeedDebuff(_modifierData.SlowAmount);
        }

        // 슬랩스틱 체류 시간
        if (_modifierData.StunOnArea == true)
        {
            // 체류 시간 기록
            _monsterEnterTimes[monster] = Time.time;
        }
    }

    protected override void OnMonsterExit(MonsterBase monster)
    {
        // 빨래당함 슬로우
        if (_modifierData.SlowOnArea == true)
        {
            // monster.RemoveSpeedDebuff(_modifierData.SlowAmount);
        }

        // 슬랩스틱 체류 시간
        if (_modifierData.StunOnArea == true)
        {
            // 나가면 체류 시간 삭제
            _monsterEnterTimes.Remove(monster);
        }
    }

    protected override void OnPlayerEnter()
    {
        // 버블버블 장판 위 플레이어 방어력
        if (_modifierData.PlayerDefenseBoost == true)
        {
            ApplyPlayerDefense();
        }
    }

    protected override void OnPlayerExit()
    {
        // 장판에서 나가면 방어력 버프 해제
        if (_modifierData.PlayerDefenseBoost == true)
        {
            RemovePlayerDefense();
        }
    }


    private void ApplyPlayerDefense()
    {
        Debug.Log($"[SoapBubble] 버블버블 방어력 버프: + {_modifierData.PlayerDefenseBonus}");
    }

    private void RemovePlayerDefense()
    {
        Debug.Log($"[SoapBubble] 버블버블 방어력 버프 해제");
    }


    // 몬스터 스턴
    private void StunMonster(MonsterBase monster)
    {
        // 해당 장판에 기절 기록 안된 적이면
        if (_stunnedMonsters.Contains(monster) == false)
        {
            // 체류 시간 체크
            if (_monsterEnterTimes.TryGetValue(monster, out float enterTime))
            {
                // 체류 시간이 일정 시간 넘으면
                if (Time.time - enterTime >= _modifierData.StunRequiredTime)
                {
                    // 스턴
                    //monster.ApplyStun(_modifierData.StunDuration);
                    // 등록해서 다시 기절 안당하게
                    _stunnedMonsters.Add(monster);
                }
            }
        }
    }

    // 소멸 시
    protected override void OnExpire()
    {
        // 발동 횟수
        int count = _runtimeFinalStat.ExtraDamageMultiplier;

        // 추가추가피해로 여러번 발동 가능
        for(int i = 0; i < count; i++)
        {
            // 거품 펑!
            // _monstersInArea에 등록된 적 모두 최대 체력 피해
            if (_modifierData.BurstOnExpire == true)
                BurstDamage();
        }

        // HashSet Clear
        base.OnExpire();    
    }


    // 소멸 시 최대 체력 피해
    private void BurstDamage()
    {
        // 와중에 null된거 삭제
        _monstersInArea.RemoveWhere(m => m == null);

        // 적 최대 체력 가져와서 BurstDamage 퍼센트의 데미지를 줌
        // float burstDamage = monster.MaxHp * _modifierData.BurstDamage;

        foreach (var monster in _monstersInArea)
        {
            //monster.TakeDamage(burstDamage);
        }
    }
}
