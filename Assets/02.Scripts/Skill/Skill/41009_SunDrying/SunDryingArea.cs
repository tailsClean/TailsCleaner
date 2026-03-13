using UnityEngine;

public class SunDryingArea : SkillArea<SunDryingModifierData>
{
    private SunDryingSkill _sunDryingSkill;

    public override void Init(ActiveSkill owner, SunDryingModifierData modifierData, Vector2 dir = default)
    {
        // 형변환 후 캐싱
        _sunDryingSkill = owner as SunDryingSkill;

        base.Init(owner, modifierData, dir);
    }

    protected override void FixedUpdate()
    {
        // 플레이어 위치 따라다님
        //transform.position = SkillManager.Instance.Player.transform.position;
        //_rigidbody.MovePosition(SkillManager.Instance.Player.transform.position);
        _rigidbody.MovePosition(GetPlayerPos());
    }

    // 틱마다
    protected override void OnTick()
    {
        base.OnTick();

        // 따스한 태양
        // 틱마다 업그레이드 스탯 증가
        if (_modifierData.DamagePerTick)
            ApplyDamagePerTick();
    }

    // 따스한 태양
    // 틱마다 업그레이드 스탯 누적
    // 추가추가피해 패시브 적용
    private void ApplyDamagePerTick()
    {
        // 따스한 태양 추가 업그레이드 스탯
        float bonus = _modifierData.DamagePerTickAmount;

        // 추가추가피해 패시브
        if (_runtimeFinalStat.ExtraMultiplier > 1)
            bonus *= _runtimeFinalStat.ExtraMultiplier;

        // 업그레이드 스탯에 더함
        _runtimeUpgradeStat.Damage += bonus;

        // 재계산
        SetDirty();
        CalculateStat();
    }

    // 소멸 시
    protected override void OnExpire()
    {
        base.OnExpire();

        //Skill에 꺼짐 알림
       if(_sunDryingSkill != null) _sunDryingSkill.OnAreaExpired();
    }
}
