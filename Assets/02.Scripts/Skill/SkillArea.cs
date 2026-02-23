using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SkillArea<TModifierData> : SkillObjectBase
    where TModifierData : class, new()
{
    protected TModifierData _modifierData;

    // 장판 범위 내 적
    protected HashSet<MonsterBase> _monstersInArea = new();

    // 최근 틱 피해 시간
    protected float _lastTickTime;

    // 장판형은 초기화 시 방향이 대체로 없음
    // 근데 속도가 추가되면서 유도될 수 있음
    public virtual void Init(ActiveSkill owner, TModifierData modifierData, Vector2 dir = default)
    {
        _monstersInArea.Clear();

        _lastTickTime = Time.time;

        _modifierData = modifierData;

        base.Init(owner, dir);
    }

    protected override void Update()
    {
        // 틱 주기 없으면 무시
        if (_runtimeFinalStat.TickRate <= 0f) return;

        // 틱 주기마다
        if (Time.time >= _lastTickTime + _runtimeFinalStat.TickRate)
        {
            // 틱 처리
            ProcessTick();
            _lastTickTime = Time.time;
        }

        base.Update();  // 수명 체크, 스노우볼링
    }


    // 충돌 감지
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent<MonsterBase>(out var monster))
        {
            _monstersInArea.Add(monster);
            OnMonsterEnter(monster);
        }
        else if (col.CompareTag("Player"))
        {
            OnPlayerEnter();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent<MonsterBase>(out var monster))
        {
            _monstersInArea.Remove(monster);
            OnMonsterExit(monster);
        }
        else if (col.CompareTag("Player"))
        {
            OnPlayerExit();
        }
    }

    protected virtual void OnMonsterEnter(MonsterBase monster) { }
    protected virtual void OnMonsterExit(MonsterBase monster) { }
    protected virtual void OnPlayerEnter() { }
    protected virtual void OnPlayerExit() { }

    // 틱 처리
    // 범위 내 적에게 피해
    // 자식의 틱 로직, 패시브 틱 로직 실행
    private void ProcessTick()
    {
        // 범위 내 적이 null인 경우 삭제
        _monstersInArea.RemoveWhere(m => m == null);

        // 범위 내 적 순회
        foreach (var monster in _monstersInArea)
        {
            // 피해
            monster.TakeDamage(_runtimeFinalStat.Damage);
            // 전용 모디파이어 로직
            OnTick(monster);
        }

        // 틱마다 스탯·크기 재적용
        CalculateStat();
        transform.localScale = Vector3.one * _runtimeFinalStat.Size;
    }

    // 틱마다 적에게 호출
    // 비누 거품의 빨래당함의 서브태그 - 집중공략 
    // 약화된 적이라면 틱마다 최대 체력 감소
    protected virtual void OnTick(MonsterBase monster) { }

    // 소멸 시 범위 내 적 정리
    protected override void OnExpire()
    {
        _monstersInArea.Clear();
    }
}
