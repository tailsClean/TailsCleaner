using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoapBubbleArea : SkillArea<SoapBubbleModifierData>
{
    // 적 체류 시작 시간
    private readonly Dictionary<MonsterBase, float> _monsterEnterTimes = new();


    private MonsterBase _trackTarget = null;    // 현재 추적 대상
    private Coroutine _searchCoroutine = null;  // 탐색 코루틴

    public override void Init(ActiveSkill owner, SoapBubbleModifierData modifierData, Vector2 dir = default)
    {
        _monsterEnterTimes.Clear();
        _trackTarget = null;
        _searchCoroutine = null;

        base.Init(owner, modifierData, dir);

        // 생성 즉시 탐색 한 번
        if (_modifierData.Tracking)
            StartSearch();
    }

    protected override void Update()
    {
        // 가장 가까운 적 추적
        if (_modifierData.Tracking == true)
            TrackClosestEnemy();

        // 스턴 체류 체크
        if (_modifierData.StunOnArea == true)
            CheckStunInArea();

        // 틱 주기 처리 추가 base를 통해 수명 체크 ,스노우 볼링
        base.Update();
    }
    
    // 가장 가까운 적 추적
    private void TrackClosestEnemy()
    {
        // 타겟 유효 확인
        if (_trackTarget == null)
        {
            // 탐색 코루틴 없으면 시작
            if (_searchCoroutine == null)
                StartSearch();
            return;
        }

        // 이동 속도
        float speed = _runtimeFinalStat.ProjectileSpeed;
        if (speed <= 0f) return;

        // 타겟 방향으로 이동
        transform.position = Vector2.MoveTowards( transform.position, _trackTarget.transform.position, speed * Time.deltaTime );
    }

    // 탐색 코루틴 시작
    private void StartSearch()
    {
        // 이미 실행중이면 무시
        if (_searchCoroutine != null) return;

        _searchCoroutine = StartCoroutine(SearchTargetCoroutine());
    }

    // 적 탐색 코루틴
    private IEnumerator SearchTargetCoroutine()
    {
        while (true)
        {
            _trackTarget = SkillManager.Instance.FindClosestMonster(transform);

            if (_trackTarget != null)
            {
                // 찾으면 비우고 종료
                // Update에서 추적 시작
                _searchCoroutine = null;
                yield break;
            }

            // 탐색 대기 시간
            yield return SkillManager.Instance.SearchInterval;
        }
    }


    protected override void OnMonsterEnter(MonsterBase monster)
    {
        // 약화 상태 체크
        // 약화 상태인지 체크 후 최대 체력 감소
        foreach (var passive in _passiveModifiers)
        {
            // if (monster.IsDebuffed)
            //     passive.OnEnterArea(monster);
        }

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


    // 장판 스턴 체류 시간 체크
    private void CheckStunInArea()
    {
        foreach (var monster in _monstersInArea)
        {
            if (monster == null) continue;

            // 기절 상태 몬스터 스킵
            // if (monster.IsStunned) continue;

            // 몬스터 체류 시간 꺼내기
            if (_monsterEnterTimes.TryGetValue(monster, out float enterTime) == false) continue;

            // 체류 시간이 기준 이상이면 기절
            if (Time.time - enterTime >= _modifierData.StunRequiredTime)
            {
                StunMonster(monster);
            }
        }
    }

    // 몬스터 스턴
    private void StunMonster(MonsterBase monster)
    {
        // 몬스터 지속 시간동안 기절 
        //monster.Stun(_modifierData.StunDuration);

        // 스턴 패시브 추가 효과 (SuperClean)
        foreach (var passive in _passiveModifiers)
            passive.OnStun(monster);
    }

    // 소멸 시
    protected override void OnExpire()
    {
        // 거품 펑!
        if (_modifierData.BurstOnExpire == true)
        {
            // 발동 횟수
            int count = _runtimeFinalStat.ExtraDamageMultiplier;

            // 추가추가피해로 여러번 발동 가능
            // _monstersInArea에 등록된 적 모두 최대 체력 피해
            for (int i = 0; i < count; i++)
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

        foreach (var monster in _monstersInArea)
        {
            // 적 최대 체력 가져와서 BurstDamage 퍼센트의 데미지를 줌
            // float burstDamage = monster.MaxHp * _modifierData.BurstDamage;

            //monster.TakeDamage(burstDamage);
        }
    }
}
