using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;

public class DebugMonsterSpawnSystem : MonoBehaviour, IMonsterSpawnSystem
{
    private const float BASE_SPAWN_RADIUS = 6f;

    [SerializeField] private DebugMonsterRegistry _registry;

    public void ApplyWave(WavePlan _wave)
    {
        if (_wave == null)
        {
            Debug.LogError("[DebugMonsterSpawnSystem]WavePlan is null. Cannot apply wave.");
            return;
        }
        
        Debug.Log($"[Spawner] ApplyWave called. waveIndex={_wave.waveIndex}");

        //테스트용 -> 웨이브마다 대표 몬스터 1개만 생성해서 눈으로 확인할 수 있도록 함. 실제 구현에서는 몬스터 스폰 계획에 따라 여러 몬스터를 생성해야 함.
        int _monsterId = GetRepresentativeMonsterId(_wave);
        SpawnDebugCube($"Wave{_wave.waveIndex}_Monster{_monsterId}", _isBoss: false);

    }

    public void SpawnBoss(int _bossId)
    {
        Debug.Log($"[Spawner] SpawnBoss called. bossId={_bossId}");
        SpawnDebugCube($"Boss_{_bossId}", _isBoss: true);
    }

    public void SpawnMidBoss(int _midBossId)
    {
        Debug.Log($"[Spawner] SpawnMidBoss called. midBossId={_midBossId}");
        SpawnDebugCube($"MidBoss_{_midBossId}", _isBoss: false);
    }

    private int GetRepresentativeMonsterId(WavePlan _wave)
    {
        if (_wave.spawns == null || _wave.spawns.Count == 0)
            return 0;

        // 가장 weightPercent가 큰 몬스터를 대표로 선택(테스트용)
        int _bestId = _wave.spawns[0].monsterId;
        int _bestWeight = _wave.spawns[0].weightPercent;

        for (int i = 1; i < _wave.spawns.Count; i++)
        {
            MonsterSpawnPlan _s = _wave.spawns[i];
            if (_s.weightPercent > _bestWeight)
            {
                _bestWeight = _s.weightPercent;
                _bestId = _s.monsterId;
            }
        }

        return _bestId;
    }

    private void SpawnDebugCube(string _name, bool _isBoss)
    {
        Vector3 _pos = GetRandomPositionAroundOrigin(BASE_SPAWN_RADIUS);

        GameObject _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.name = _name;
        _cube.transform.position = _pos;

        if (_isBoss)
            _cube.transform.localScale = Vector3.one * 2f;

        if (_registry != null)
            _registry.RegisterMonster(_cube);
        else
            Debug.LogWarning("[Spawner] Registry is null. Spawned objects won't be cleaned up.");
    }

    private Vector3 GetRandomPositionAroundOrigin(float _radius)
    {
        Vector2 _r = Random.insideUnitCircle * _radius;
        return new Vector3(_r.x, 0f, _r.y);
    }

}
