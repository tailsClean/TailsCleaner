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

    // 현재 적용 중인 전역 강화 수치
    private float _bonusStrength = 0f;

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

            // 신규 스폰 몬스터에게 현재까지 누적된 강화 적용
            if (_bonusStrength > 0)
            {
                monster.ApplyEnhancement(_bonusStrength);
            }

            OnMonsterSpawned?.Invoke(monster);
        }
    }
    public void ApplyAllEnemyEnhance(float bonusStrength)
    {
        _bonusStrength = bonusStrength;

        foreach (var monster in activeMonsters)
        {
            monster.ApplyEnhancement(bonusStrength);
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
}