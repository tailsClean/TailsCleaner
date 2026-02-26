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

    protected override void Update()
    {
        // 플레이어 위치 따라다님
        transform.position = SkillManager.Instance.Player.transform.position;

        // 틱 처리, 수명 체크, 스노우볼링(UpdateDurationTick)
        base.Update();

        // 따스한 태양
        // 틱 주기마다 업그레이드 스탯 증가
        // 틱 데미지 이후 증가됨
        if (_modifierData.DamagePerTick
            && _runtimeFinalStat.TickRate > 0f
            && Time.time >= _lastTickTime + _runtimeFinalStat.TickRate)
        {
            ApplyDamagePerTick();
        }

    }

    // 따스한 태양
    // 틱마다 업그레이드 스탯 누적
    // 추가추가피해 패시브 적용
    private void ApplyDamagePerTick()
    {
        // 따스한 태양 추가 업그레이드 스탯
        float bonus = _modifierData.DamagePerTickAmount;

        // 추가추가피해 패시브
        if (_runtimeFinalStat.ExtraDamageMultiplier > 1)
            bonus *= _runtimeFinalStat.ExtraDamageMultiplier;

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
