using UnityEngine;

//  TowelSwingArea / MopSwingArea / CrescentSwingArea 베이스
public abstract class SwingArea<TModifierData> : SkillArea<TModifierData> where TModifierData : SwingModifierData, new()
{
    [Header("플레이어와 거리")]
    [SerializeField] private float _swingDistance = 1.5f;

    private Vector2 _dirOffset;

    // 초기화
    public void InitSwing(ActiveSkill owner, TModifierData modifierData, Vector2 dir)
    {
        // 방향의 옵셋
        _dirOffset = dir.normalized * _swingDistance;

        // 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 위치 - 플레이어 위치에 방향 옵셋만큼
        transform.position = GetPlayerPos() + _dirOffset;

        base.Init(owner, modifierData, dir);
    }

    protected override void Update()
    {
        // 플레이어 위치 따라가기
        transform.position = GetPlayerPos() + _dirOffset;

        base.Update();
    }

    // 장판 범위 진입
    // 젖은 걸레 (슬로우)
    // 어질어질 (기절)
    protected override void OnMonsterEnter(MonsterBase monster)
    {
        // 젖은 걸레
        if (_modifierData.SlowOnHit)
        {
            // monster.ApplySlow(_modifierData.SlowAmount, _modifierData.SlowDuration);
            Debug.Log($"[SwingArea] 젖은 걸레 - 슬로우 {_modifierData.SlowAmount * 100f}% / {_modifierData.SlowDuration}s");

            // 집중공략 (약화 적 최대체력 감소)
            foreach (var passive in _passiveModifiers)
                passive.OnEnterArea(monster);
        }

        // 어질어질
        if (_modifierData.StunOnHit)
        {
            // monster.ApplyStun(_modifierData.StunDuration);
            Debug.Log($"[SwingArea] 어질어질 - 기절 {_modifierData.StunDuration}s");

            // SuperClean (기절 시 추가 슬로우 5초)
            foreach (var passive in _passiveModifiers)
                passive.OnStun(monster);
        }
    }

    // 틱 처리
    // 패링 (탄환 제거)
    protected override void OnTick(MonsterBase monster)
    {
        if (_modifierData.BulletClear)
        {
            // 범위 내 적 탄환 Destroy (풀 반환)
            // NimbleBlock 탄환 제거 시 방어막 충전
            Debug.Log("[SwingArea] 패링 - 탄환 제거");
        }
    }

    // 플레이어 위치
    private Vector2 GetPlayerPos()
        => (Vector2)SkillManager.Instance.Player.transform.position;
}
