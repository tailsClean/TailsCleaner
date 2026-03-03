using UnityEngine;

public class WaterBombArea : SkillArea<WaterBombModifierData>
{
    [Header("투사체 상태 오브젝트")]
    [SerializeField] GameObject _fallObject;
    [SerializeField] GameObject _landObject;

    private bool _isLanded = false;             // 착지 상태
    private Vector2 _startPos;                  // 낙하 시작 위치
    private Vector2 _targetPos;                 // 목표 지점 위치
    private float _fallStartTime;               // 낙하 시작 시간
    private WaterBombSkill _waterBombSkill;


    public override void Init(ActiveSkill owner, WaterBombModifierData modifierData, Vector2 targetPos)
    {
        // 형변환
        _waterBombSkill = owner as WaterBombSkill;

        // 목표 지점
        _targetPos = targetPos;
        // 시작 위치 (목표 지점에서 일정 거리 위)
        _startPos = targetPos + Vector2.up * _waterBombSkill.FallHeight;
        // 위치 갱신
        transform.position = _startPos;
        // 낙하 시작 시간
        _fallStartTime = Time.time;

        base.Init(owner, modifierData, Vector2.zero);
    }


    protected override void Update()
    {
        // 낙하 중
        // 수명, 틱, 스노우볼링 전부 차단
        if (_isLanded == false)
        {
            Fall();
            return;
        }

        // 착지 후
        base.Update();
    }
    
    
    // 수직 낙하
    private void Fall()
    {
        // 시간 기반 Lerp
        float t = (Time.time - _fallStartTime) / _waterBombSkill.FallDuration;

        transform.position = Vector2.Lerp(_startPos, _targetPos, t);

        // 낙하 완료
        if (t >= 1f)
        {
            transform.position = _targetPos;
            OnLand();
        }
    }
    
    // 착지 처리
    private void OnLand()
    {
        _isLanded = true;

        // 투사체 상태 오브젝트 변경
        _fallObject.SetActive(false);
        _landObject.SetActive(true);

        // 착지 시점부터
        // 수명, 틱 타이머 시작
        _createTime = Time.time;
        _lastTickTime = Time.time;

        // 물바다
        // 8방향 추가 투사체 발사
        if (_modifierData.Splash)
            if(_waterBombSkill != null) _waterBombSkill.SpawnSplash(transform.position, _runtimeFinalStat);

        // 소용돌이
        // WaterBombSkill에서 생성
        if (_modifierData.Vortex)
            if (_waterBombSkill != null) _waterBombSkill.SpawnVortexArea(transform.position, _runtimeFinalStat, _modifierData, _passiveModifiers);
    }


    // 장판에 적 투사체 진입 시
    protected override void OnBulletEnter(MonsterProjectile projectile)
    {
        // 폭발은 예술이다
        if (_modifierData.BulletClear == false) return;

        // 투사체 삭제
        // 풀 반환으로 교체
        Destroy(projectile.gameObject);

        // 방어막 획득
        // SkillManager.Instance.Player.AddShield(1);
        Debug.Log("[WaterBombArea] 폭발은 예술이다 - 투사체 삭제 (방어막 획득)");
    }
}
