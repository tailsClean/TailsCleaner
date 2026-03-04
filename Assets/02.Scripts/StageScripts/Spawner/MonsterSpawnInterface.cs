using UnityEngine;

// WaveScheduler/StageState가 결정한 웨이브 정보를 받아
// 실제 몬스터를 생성(스폰)하는 실행 시스템.
// 스폰 위치/패턴(Random/Squad/Circle) 적용은 구현체가 담당.
public interface IMonsterSpawnSystem
{
    void ApplyWave(WavePlan _wave);
    void SpawnMidBoss(int _midBossId);
    void SpawnBoss(int _bossId);

    //보스 소환 시 스폰 정지
    void SetSpawningEnabled(bool _isenabled);
}

// 필드에 존재하는 몬스터들을 추적/제어하는 레지스트리.
// 보스 시작/스테이지 종료 등 특정 상황에서 몬스터 전체 정리가 필요할 때 사용.
public interface IMonsterRegistry
{
    void KillAllMonsters();
    void ClearBossMark();
}