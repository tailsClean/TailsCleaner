using UnityEngine;

// 비누 던지기 모디파이어
public class SoapModifierData
{
    public bool Retracking = false;
}

public class SoapThrowSkill : GenericActiveSkill<SoapProjectile, SoapModifierData>
{
    // 스킬 발동
    protected override void Active()
    {
        // 투사체 수
        int count = Mathf.Max(1, _finalStat.ProjectileCount);

        for (int i = 0; i < count; i++)
        {
            // 랜덤 원 방향 (임시)
            // 공격 방향에서 가까운 적으로 변경해야 함
            Vector2 randomDir = Random.insideUnitCircle.normalized;

            // 발사
            SpawnSoap(randomDir);
        }
    }

    // 비누 투사체 생성
    private void SpawnSoap(Vector2 dir)
    {
        // 비누 생성
        SoapProjectile soap = Instantiate(_skillPrefabComponent, transform.position, Quaternion.identity);

        // 데이터 주입
        soap.Init(_finalStat, dir, ModifierData);
    }
}
