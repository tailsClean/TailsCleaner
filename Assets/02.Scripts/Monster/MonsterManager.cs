using UnityEngine;
using System.Collections.Generic;
using System;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    // 현재 필드에 살아있는 모든 몬스터 리스트 
    public List<MonsterBase> activeMonsters = new List<MonsterBase>();

    // 몬스터가 생성될 때 발생할 이벤트
    public event Action<MonsterBase> OnMonsterSpawned;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 몬스터의 OnSpawn에서 이 함수를 호출
    public void RegisterMonster(MonsterBase monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);

            // 몬스터가 생성되어 필드에 등록될 때 호출
            // 이 신호를 받은 다른 클래스 들이 각자 필요한 처리를 시작
            OnMonsterSpawned?.Invoke(monster);

            Debug.Log($"[MonsterManager] {monster.name} 등록 완료. 현재 개수: {activeMonsters.Count}");
        }
    }

    // 몬스터가 죽거나 사라질 때 호출
    // 관리 리스트에서 제거하여 더 이상 추격이나 공격 대상이 되지 않도록 정리
    public void UnregisterMonster(MonsterBase monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }
    }

    // 필드 전체 몬스터에게 명령 내리기 (예: 모든 적 기절)
    public void ExecuteGlobalStun(float duration)
    {
        foreach (var monster in activeMonsters)
        {
            monster.ApplyStun(duration);
        }
    }
}