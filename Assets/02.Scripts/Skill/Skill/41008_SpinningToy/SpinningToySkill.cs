using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpinningToySkill : ActiveSkill<SpinningToyProjectile, SpinningToyModifierData>
{
    public enum TOY_TYPE
    {
        Default = 0,    // 기본 회전 장난감
        Train,          // 기차
        Top,            // 팽이
        Moon,           // 달
        Duck,           // 오리 
        Pirate,         // 해적선
        Shark,          // 상어
    }

    public const float ORBIT_SPEED_MULTI = 120f;

    [Header("공전 설정")]
    [SerializeField] float _orbitRadius = 2.5f;

    protected override void Active()
    {
        // 스폰할 투사체 리스트
        List<TOY_TYPE> spawnList = BuildSpawnList();

        // 투사체 수
        int count = spawnList.Count;

        for (int i = 0; i < count; i++)
        {
            // 투사체 수만큼 각 나누기
            float angle = i * 360f / count;
            Vector2 spawnPos = (Vector2)transform.position + GetOrbitOffset(angle);

            // 투사체 생성
            SpinningToyProjectile toy = Instantiate(_skillObjectPrefab, spawnPos, Quaternion.identity);
            
            // 공전상태로 초기화
            toy.InitOrbit(this, _modifierData, spawnList[i], angle, _orbitRadius);
        }
    }

    // 스폰할 투사체 리스트 빌드
    // 양손잡이로 투사체 두배되었을 때 비율 알아내서 나누는 과정
    private List<TOY_TYPE> BuildSpawnList()
    {
        // 기본 투사체 수
        int basePart = _baseStat.ProjectileCount * _commonStat.ProjectileCount;

        // 업그레이드 투사체 수
        int upgradePart = 0;
        foreach (var (type, count) in _modifierData.AddedToys)
            upgradePart += count;

        // 총 투사체 수
        int totalPart = basePart + upgradePart;

        // 뻥튀기 된 비율
        // 기본 + 업그레이드 투사체 합과 최종 스탯 개수 차이로 배율 알아냄
        // 최종이 10개인데 총 투사체가 5개면 2배된 것
        float finalProjectileMul = totalPart > 0
            ? (float)_finalStat.ProjectileCount / totalPart
            : 1f;

        // 스폰할 투사체 리스트
        var list = new List<TOY_TYPE>();

        // 기본 장난감을 뻥튀기 배율만큼 넣기
        int finalBaseCount = Mathf.RoundToInt(basePart * finalProjectileMul);
        for (int i = 0; i < finalBaseCount; i++)
            list.Add(TOY_TYPE.Default);

        // 특수 장난감들도 각각 배율만큼 넣기
        foreach (var (type, count) in _modifierData.AddedToys)
        {
            int typeCount = Mathf.RoundToInt(count * finalProjectileMul);
            for (int i = 0; i < typeCount; i++)
                list.Add(type);
        }

        // 혹시나 0개 들어간 특수 장난감은 디폴트로 넣음
        if (list.Count == 0)
            list.Add(TOY_TYPE.Default);

        return list;
    }


    // 복사본 버스트 (물놀이 끝 + 추가추가피해)
    public void SpawnBurstCopy(Vector2 spawnPos, Vector2 dir)
    {
        SpinningToyProjectile copy = Instantiate(_skillObjectPrefab, spawnPos, Quaternion.identity);
        copy.Init(this, _modifierData, dir);
    }

    // 투사체 수에 따라 각 벌리기
    private Vector2 GetOrbitOffset(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * _orbitRadius;
    }
}
