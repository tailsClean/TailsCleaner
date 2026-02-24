using UnityEngine;

public class SoapThrowSkill : ActiveSkill<SoapThrowProjectile, SoapThrowModifierData>
{
    // 스킬 발동
    protected override void Active()
    {
        // 투사체 수
        int count = Mathf.Max(1, _finalStat.ProjectileCount);

        for (int i = 0; i < count; i++)
        {
            // 공격 방향에서 가장 가까운 적 위치 가져오기
            Transform targetTrans = SkillManager.Instance.Player.AttackTarget;
            Vector2 dir;

            // 타겟이 있다면
            if (targetTrans != null)
                dir = (targetTrans.position - transform.position).normalized;

            // 타겟이 없다면
            else
            {
                // 마지막 공격 방향
                dir = SkillManager.Instance.Player.AttackDir.normalized;

                // 혹시나 시작 시 안건들면 0 이니까 
                if (dir == Vector2.zero)
                    dir = new Vector2(SkillManager.Instance.Player.transform.localScale.x, 0);
            }

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
