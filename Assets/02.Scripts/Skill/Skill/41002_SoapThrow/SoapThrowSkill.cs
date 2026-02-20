using UnityEngine;

// 비누 던지기 모디파이어
public class SoapThrowModifierData
{
    public bool Retracking = false;     // 재추적
    public bool PierceDamage = false;   // 관통 추가 피해
    public bool PierceSpeed = false;    // 관통 추가 속도
    public bool RemovePierce = false;   // 관통 제거

    // Config에서 설정
    public float DamagePerPierce = 0f;  
    public float SpeedPerPierce = 0f;
}

public class SoapThrowSkill : GenericActiveSkill<SoapThrowProjectile, SoapThrowModifierData>
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
        // 바라볼 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 비누 생성 (풀링 필요)
        SoapThrowProjectile soap = Instantiate(_skillPrefabComponent, transform.position, Quaternion.Euler(0, 0, angle));

        // 데이터 주입
        soap.Init(this, ModifierData, dir);
    }
}
