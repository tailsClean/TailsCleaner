using UnityEngine;

public class SpawnTest : MonoBehaviour
{
    private void Start()
    {
        // MonsterManager의 방송 채널(이벤트)에 귀를 기울입니다.
        if (MonsterManager.Instance != null)
        {
            MonsterManager.Instance.OnMonsterSpawned += (monster) => {
                Debug.Log($"<color=yellow>[테스트 성공]</color> 수신! 방금 나온 몬스터: {monster.name}");
            };
            // Debug.Log("테스트 준비 완료: 방송을 기다리는 중...");
        }
    }
}