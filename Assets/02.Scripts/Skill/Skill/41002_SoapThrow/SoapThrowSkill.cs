using UnityEngine;

public class SoapThrowSkill : ActiveSkill<SoapThrowProjectile, SoapThrowModifierData>
{
    // 스킬 발동
    protected override void OnActive(int index, int totalCount)
    {
        // 혹시나 비어있으면 스킵
        if (_currentTarget == null) return;

        // 방향
        Vector2 dir = (_currentTarget.position - transform.position).normalized;

        // 발사
        SpawnSoap(dir);
    }

    // 비누 투사체 생성
    private void SpawnSoap(Vector2 dir)
    {
        // 바라볼 각도
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 비누 생성
        //SoapThrowProjectile soap = Instantiate(_skillObjectPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        SoapThrowProjectile soap = SpawnFromPool<SoapThrowProjectile>(_skillObjectPrefab, transform.position, Quaternion.Euler(0f, 0f, angle));

        // 초기화
        if(soap != null) soap.Init(this, _modifierData, dir);
    }
}
