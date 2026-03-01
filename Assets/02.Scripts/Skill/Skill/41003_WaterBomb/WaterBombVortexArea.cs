using System.Collections.Generic;
using UnityEngine;

public class WaterBombVortexArea : MonoBehaviour
{
    private float _pullInterval;    // 당기기 주기
    private float _lastPullTime;    // 최근 당기기 시간
    private float _duration;        // 지속 시간
    private int _remainPullCount;   // 남은 당기기 횟수

    private bool _bulletClear;      // 폭발은 예술이다 상태
    private bool _expired;          // 만료 상태

    private List<PassiveModifier> _passiveModifiers;    // 패시브

    // 범위 내 몬스터
    private HashSet<MonsterBase> _monstersInArea = new();

    // 범위 내 적 투사체
    private HashSet<MonsterProjectile> _bulletsInArea = new();


    public void Init(float pullInterval, int pullCount, float size, List<PassiveModifier> passives, bool bulletClear)
    {
        _pullInterval = pullInterval;
        _remainPullCount = pullCount;
        _lastPullTime = Time.time;
        _bulletClear = bulletClear;
        _expired = false;
        _passiveModifiers = new List<PassiveModifier>(passives);

        transform.localScale = Vector3.one * size;
    }

    private void Update()
    {
        if (_expired) return;

        // 남은 횟수만큼 끌어당기기
        if (_remainPullCount > 0 && Time.time >= _lastPullTime + _pullInterval)
        {
            // 끌어당기고
            ProcessPull();
            // 횟수 차감
            _remainPullCount--;
            // 주기 갱신
            _lastPullTime = Time.time;
        }

        // 남은 횟수 없으면 비활성화
        if (_remainPullCount <= 0)
        {
            // 만료 상태 전환
            _expired = true;
            // 해시셋 정리
            _monstersInArea.Clear();
            _bulletsInArea.Clear();
            Destroy(gameObject);    // 나중에 풀반환
            return;
        }
    }


    // 끌어당기기
    private void ProcessPull()
    {
        // null 지우기
        _monstersInArea.RemoveWhere(monster => monster == null);
        _bulletsInArea.RemoveWhere(bullet => bullet == null);

        foreach (var monster in _monstersInArea)
        {
            // 범위 내 적 투사체 끌어당기기
            // monster.Pull(transform.position)

            // 집중공략 패시브
            // 끌어당겨진 적 약화상태면 최대 체력 5% 감소
        }
    }

    // 폭발은 예술이다
    // 투사체 진입 즉시 삭제 + 방어막 획득
    private void OnTriggerEnter2D(Collider2D col)
    {
        // 몬스터
        if (col.CompareTag("Monster"))
        {
            // 컴포넌트
            if (col.TryGetComponent<MonsterBase>(out var monster))
            {
                // 해시셋에 등록
                _monstersInArea.Add(monster);
            }
        }
        // 몬스터 투사체
        else if (col.CompareTag("MonsterBullet"))
        {
            // 컴포넌트
            if (col.TryGetComponent<MonsterProjectile>(out var projectile))
            {
                // 폭발은 예술이다
                if(_bulletClear == true)
                {
                    // 풀반환으로 교체
                    Destroy(projectile.gameObject);

                    // 플레이어 방어막 획득
                    // SkillManager.Instance.Player.AddShield(1);
                    Debug.Log("[WaterBombVortex] 폭발은 예술이다 - 투사체 삭제");
                    // 해시셋 추가는 스킵
                    return;
                }

                // 폭발은 예술이다 비활성화면
                // 해시셋에 추가
                _bulletsInArea.Add(projectile);

            }
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    { // 몬스터
        if (col.CompareTag("Monster"))
        {
            // 컴포넌트
            if (col.TryGetComponent<MonsterBase>(out var monster))
            {
                // 해시셋에서 삭제
                _monstersInArea.Remove(monster);
            }
        }
        // 몬스터 투사체
        else if (col.CompareTag("MonsterBullet"))
        {
            // 컴포넌트
            if (col.TryGetComponent<MonsterProjectile>(out var projectile))
            {
                // 해시셋에서 삭제
                _bulletsInArea.Remove(projectile);
            }
        }
    }
}
