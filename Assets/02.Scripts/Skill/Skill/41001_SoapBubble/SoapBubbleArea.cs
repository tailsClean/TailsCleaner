using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoapBubbleArea : SkillArea<SoapBubbleModifierData>
{
    [Header("거품 펑 오브젝트")]
    [SerializeField] SkillAnimator _burstAnimator;

    // 적 체류 시작 시간
    private readonly Dictionary<MonsterBase, float> _monsterEnterTimes = new();


    private MonsterBase _trackTarget = null;    // 현재 추적 대상
    private float _searchTimer = 0f;            // 탐색 타이머

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Init(ActiveSkill owner, SoapBubbleModifierData modifierData, Vector2 dir = default)
    {
        _monsterEnterTimes.Clear();
        _trackTarget = null;

        if (_burstAnimator != null)
        {
            _burstAnimator.gameObject.SetActive(false);
            _burstAnimator.ResetState();
        }

        // 시작하면 바로 탐색하게 시간 꽉채우기
        _searchTimer = SkillManager.SEARCH_INTERVAL;

        base.Init(owner, modifierData, dir);
    }

    protected override void Update()
    {
        // 추적 유효성 검사 및 탐색
        if (_modifierData.Tracking == true)
            CheckAndSearchTarget();

        // 스턴 체류 체크
        if (_modifierData.StunOnArea == true)
            CheckStunInArea();

        // 틱 주기 처리 추가 base를 통해 수명 체크 ,스노우 볼링, 이동
        base.Update();
    }
    protected override void FixedUpdate()
    {
        if (_expired) return;

        // 가장 가까운 적 추적
        if (_modifierData.Tracking == true)
            MoveToTarget();
    }

    // 추적 유효성 검사 및 탐색
    private void CheckAndSearchTarget()
    {
        // 타겟이 없거나, 비활성화됐거나, 죽었으면 다시 탐색
        if (_trackTarget == null || _trackTarget.gameObject.activeInHierarchy == false || _trackTarget.hp <= 0)
        {
            _trackTarget = null;

            // 탐색 타이머 증가
            _searchTimer += Time.deltaTime;

            // 탐색 간격마다
            if (_searchTimer >= _modifierData.TrackingInterval)
            {
                // 타이머 초기화
                _searchTimer = 0f;

                // 버퍼 배열에서 가장 가까운 몬스터
                _trackTarget = SkillManager.Instance.FindClosestMonster(_rigidbody.position);
            }
        }
    }

    // 이동
    private void MoveToTarget()
    {
        // 타겟 있을 때 까지 이동 스킵
        if (_trackTarget == null) return;

        // 이동 속도
        float speed = _runtimeFinalStat.ProjectileSpeed;
        if (speed <= 0f) return;

        // 타겟 방향으로 물리 이동
        //transform.position = Vector2.MoveTowards(transform.position, _trackTarget.transform.position, speed * Time.deltaTime);
        Vector2 nextPos = Vector2.MoveTowards(_rigidbody.position, _trackTarget.transform.position, speed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(nextPos);
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


    protected override void ExpireObject()
    {
        if (_expired == true) return;
        _expired = true;

        // 만료 로직
        OnExpire();

        // 콜라이더 끄기
        if (_collider != null) _collider.enabled = false;

        // 거품 펑 모디파이어
        if (_modifierData.BurstOnExpire == true && _burstAnimator != null)
        {
            // 오브젝트 켜기
            _burstAnimator.gameObject.SetActive(true);

            // 발동 연출 시작
            _burstAnimator.StartSequence(0f);

            // 펑 종료 연출 후 콜백
            _burstAnimator.RequestExpire(() =>
            {
                // 펑 오브젝트 끄기
                _burstAnimator.gameObject.SetActive(false);

                // 펑 끝나고 종료 연출
                ExpireSequence();
            });
        }
        else
        {
            // 거품 펑 모디파이어 없거나 애니메이터 없으면
            // 바로 종료 연출
            ExpireSequence();
        }
    }


    // 소멸 시
    protected override void OnExpire()
    {
        // 거품 펑!
        if (_modifierData.BurstOnExpire == true)
        {
            // 발동 횟수
            int count = _runtimeFinalStat.ExtraMultiplier;

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
        _monstersInArea.RemoveWhere(m => m == null || m.gameObject.activeInHierarchy == false);

        foreach (var monster in _monstersInArea)
        {
            // 적 최대 체력 가져와서 BurstDamage 퍼센트의 데미지를 줌
            // float burstDamage = monster.MaxHp * _modifierData.BurstDamage;

            //monster.TakeDamage(burstDamage);
        }
    }
}
