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
            // 랜덤 원 방향
            Vector3 randomDir = Random.insideUnitCircle.normalized;

            // 발사
            SpawnSoap(randomDir);
        }

        Debug.Log("비누 던지기 실행");
    }

    // 비누 투사체 생성
    private void SpawnSoap(Vector3 dir)
    {
        // 프리팹 생성
        GameObject obj = Instantiate(_skillPrefab, transform.position, Quaternion.identity);
        SoapProjectile soap = obj.GetComponent<SoapProjectile>();

        if (soap != null)
        {
            // 최종 스탯, 방향, ModifierData 넘김
            //soap.Init(_finalStat, dir, ModifierData);
        }
    }
}
