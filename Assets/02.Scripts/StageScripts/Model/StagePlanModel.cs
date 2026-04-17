using UnityEngine;
using System.Collections.Generic;

//파싱 제작 후 주입 받을 데이터 전송 객체(DTO)
public class StagePlan
{
    public int stageId;

    public int mainLimitSeconds;
    public int bossLimitSeconds;

    public int entryEnergy;

    public int bossId;
    public int specialGroupId;

    public IReadOnlyList<WavePlan> wavePlans;
    public IReadOnlyList<SpecialMonsterRow> specialRows;

    public float stageHpModifier;
    public float stagePowerModifier;

    public float towerHpModifier;
    public float towerPowerModifier;

    public string mapResource; // 추가
    public int expGain;

    public bool isFinalBoss; // 탑의 최종 보스인지
}

// 웨이브의 특정 시간 구간에 어떤 스폰 구성이 활성인지 정의
public class WavePlan
{
    public int waveIndex;
    public int startTimeSeconds;
    public int endTimeSeconds;

    public SpawnPattern spawnPattern;
    public IReadOnlyList<MonsterSpawnPlan> spawns;

    public int midBossId; // 웨이브 중간 보스가 있을 경우
    
    public float waveHpModifier;
    public float wavePowerModifier;
    public float waveExpMultiply;
}

// 웨이브 내부 몬스터 스폰 구성
public class MonsterSpawnPlan
{
    public int monsterId;
    public int spawnAmount;
    public float spawnIntervalSeconds; // 몬스터 간 스폰 간격
    
}

// 몬스터 소환 패턴
public enum SpawnPattern
{
    Random = 1,
    Squad = 2,
    Circle = 3,
}