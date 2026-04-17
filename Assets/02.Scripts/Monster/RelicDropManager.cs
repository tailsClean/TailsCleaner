using UnityEngine;
using System.Collections.Generic;

public class RelicDropManager : MonoBehaviour
{
    public static RelicDropManager Instance { get; private set; }

    [SerializeField] private PoolObject relicItemPrefab;

    private void Awake() => Instance = this;

    /// <summary>
    /// 몬스터 처치 시 호출되어 확률에 따라 유물을 드랍합니다.
    /// </summary>
    public void TryDropRelic(int monsterId, Vector2 spawnPos)
    {
        // Monster 테이블(ItemDB)에서 몬스터 정보를 가져옴
        // Monster 클래스는 ItemDataBase를 상속받아야 TryGetData 사용 가능
        if (!ItemDB.TryGetData<Monster>(monsterId, out var monsterData)) return;

        // 해당 몬스터의 drop_group_id를 사용하여 실제로 드랍될 유물 ID를 확률적으로 결정
        int relicId = GetDropRelicId(monsterData.drop_group_id);

        // 드랍되지 않았거나, 잘못된 ID인 경우 종료
        if (relicId <= 0) return;

        // 인벤토리 소지 여부 확인 (중복 획득 방지)
        // ItemManager에 구현된 HasRelic 통로를 이용
        if (ItemManager.Instance.HasRelic(relicId))
        {
            Debug.Log($"[Relic] ID:{relicId}는 이미 보유 중이라 드랍하지 않습니다.");
            return;
        }

        // 필드에 유물 아이템 스폰 
        if (ObjectPoolManager.Instance != null)
        {
            var relicObj = ObjectPoolManager.Instance.Spawn(relicItemPrefab, spawnPos, Quaternion.identity);
            if (relicObj.TryGetComponent<FieldRelicItem>(out var item))
            {
                // 생성된 아이템에 유물 ID 설정
                item.Setup(relicId);
            }
        }
    }

    /// <summary>
    /// 드랍 그룹 ID를 기반으로 MonsterDropSO에서 무작위 유물 ID를 반환
    /// </summary>
    private int GetDropRelicId(int dropGroupId)
    {
        // DataManager에서 런타임에 로드된 MonsterDropSO를 가져옴
        var table = DataManager.Instance.GetSOData<MonsterDropSO>();

        if (table == null || table.dataList == null)
        {
            Debug.LogError("MonsterDropSO를 찾을 수 없거나 dataList가 비어있습니다.");
            return 0;
        }

        // 전체 드랍 리스트에서 현재 몬스터의 drop_group_id와 일치하는 항목들만 필터링
        List<MonsterDrop> groupItems = table.dataList.FindAll(d => d.drop_group_id == dropGroupId);

        if (groupItems.Count == 0) return 0;

        // 확률 계산 로직 (시트의 drop_rate 합산 기준)
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (var drop in groupItems)
        {
            cumulative += drop.drop_rate;
            if (randomValue <= cumulative)
            {
                return drop.id; // 당첨된 유물 ID 반환
            }
        }

        return 0; // 확률에 당첨되지 않음
    }
}