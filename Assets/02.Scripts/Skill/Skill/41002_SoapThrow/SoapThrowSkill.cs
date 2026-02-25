using UnityEngine;

public class SoapThrowSkill : ActiveSkill<SoapThrowProjectile, SoapThrowModifierData>
{
    // 스킬 발동
    protected override void Active()
    {
        // 투사체 수
        int count = Mathf.Max(1, _finalStat.ProjectileCount);

        // 대상 트랜스폼
        Transform targetTrans = SkillManager.Instance.Player.AttackTarget;

        // 방향
        Vector2 dir = (targetTrans.position - transform.position).normalized;

        for (int i = 0; i < count; i++)
        {
            // 발사
            SpawnSoap(dir);
        }
    }

    // 비누 투사체 생성
    private void SpawnSoap(Vector2 dir)
    {
        // 바라볼 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 비누 생성 (풀링 필요)
        SoapThrowProjectile soap = Instantiate(_skillObjectPrefab, transform.position, Quaternion.Euler(0, 0, angle));

        // 데이터 주입
        soap.Init(this, _modifierData, dir);
    }
}
