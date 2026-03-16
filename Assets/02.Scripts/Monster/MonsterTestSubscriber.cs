using UnityEngine;

public class MonsterTestSubscriber : MonoBehaviour
{
    private void Start()
    {
        //  MonsterManager의 방송 채널(이벤트)에 주파수를 맞춤 (구독)
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.OnMonsterSpawned += HandleMonsterSpawned;
            Debug.Log("테스트 구독자: 방송 청취 시작!");
        }
    }

    // 실제로 방송(알림)이 왔을 때 실행될 함수
    private void HandleMonsterSpawned(MonsterBase monster)
    {
        Debug.Log($"<color=cyan>[테스트 성공]</color> 새로운 몬스터 발견: {monster.name}!");
        // 여기서 실제로 스킬을 걸거나 이펙트를 터뜨리는 로직 들어가는 곳.
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위해 파괴될 때 구독 해제
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.OnMonsterSpawned -= HandleMonsterSpawned;
        }
    }
}