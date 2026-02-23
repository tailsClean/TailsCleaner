using UnityEngine;
using UnityEngine.Rendering;

public class RuleBasedMonsterSpawner : MonoBehaviour, IMonsterSpawnSystem
{
    private const float MIN_SPAWN_DISTANCE = 10f; //플레이어로부터 최소한의 스폰 거리
    private const float MAX_SPAWN_DISTANCE = 20f; //플레이어로부터 최대한의 스폰 거리

    private const int SPAWN_PER_SECOND = 5; //초당 스폰할 몬스터 수
    private const float SQUAD_RADIUS = 3f; //스쿼드 내 몬스터들이 모이는 반경
    private const int CIRCLE_SLOTS = 12; //원형 스폰 시 슬롯 수

    private const int NO_BOSS_ID = -1; // 보스가 없는 경우의 ID

    [Header("References")]
    [SerializeField] private Transform _playerTransform; //플레이어 위치 참조
    [SerializeField] private MonsterRegistry _registry;

    [Header("Prefabs")]
    [SerializeField] private MonsterBase _normalMonsterPrefab;
    [SerializeField] private MonsterBase _midBossPrefab;
    [SerializeField] private MonsterBase _bossPrefab;

    private WavePlan _currentWave;

    private bool _isSpawningEnabled = true; //스폰 활성화 여부
    private float _spawnAccmulator; //스폰 타이밍 누적값

    //패턴 캐시
    private Vector2 _squadCenter; //스쿼드 중심 위치
    private int _circleIndex; //원형 스폰 슬롯 인덱스

    public void SetSpawningEnabled(bool _isenabled)
    {
        _isSpawningEnabled = _isenabled;
    }

    public void ApplyWave(WavePlan _wave)
    {
        _currentWave = _wave;

        _spawnAccmulator = 0f;
        _circleIndex = 0;

        if (_currentWave != null && _currentWave.spawnPattern == SpawnPattern.Squad)
        {
            _squadCenter = GetRandomRingPosition();
        }
    }

    private void Update()
    {
        if (!_isSpawningEnabled)
        {
            return;
        }
        if (_playerTransform == null || _registry == null)
        {
            return;
        }
        if (_normalMonsterPrefab == null)
        {
            return;
        }
        if (_currentWave == null)
        {
            return;
        }
        if (!_registry.CanSpawnMore())
        {
            return;
        }

        _spawnAccmulator += Time.deltaTime;
        float _interval = 1f / SPAWN_PER_SECOND;

        while (_spawnAccmulator >= _interval)
        {
            _spawnAccmulator -= _interval;

            if (!_registry.CanSpawnMore())
            {
                break;
            }
        }

        SpawnOneFromWave();
    }

    public void SpawnBoss(int _bossId)
    {
        if (_bossPrefab == null)
        {
            return;
        }

        SpawnPrefab(_bossPrefab, $"Boss_{_bossId}", _isBoss: true);
    }

    public void SpawnMidBoss(int _midBossId)
    {
        if (_midBossId == NO_BOSS_ID)
        {
            return;
        }

        if (_midBossPrefab == null)
        {
            return;
        }

        SpawnPrefab(_midBossPrefab, $"MidBoss_{_midBossId}", _isBoss: true);
    }

    private void SpawnOneFromWave()
    {
        // 지금 단계에서는 monsterId별 prefab 분기 없이 “일반 몬스터 프리팹” 하나로 스폰
        // monsterId는 향후 몬스터팀이 데이터 주입/프리팹 교체로 처리할 예정
        int _monsterId = PickMonsterIdByWeight(_currentWave);
        SpawnPrefab(_normalMonsterPrefab, $"Monster_{_monsterId}", _isBoss: false);
    }

    private void SpawnPrefab(MonsterBase _prefab, string _name, bool _isBoss)
    {
        Vector2 _pos2D = GetSpawnPositionByPattern(_isBoss);
        Vector3 _spawnPos = new Vector3(_pos2D.x, _pos2D.y, 0f);

        MonsterBase _monster = Instantiate(_prefab, _spawnPos, Quaternion.identity);
        _monster.name = _name;

        // --- MonsterBase 요구사항 충족(2D) ---
        _monster.is3DMode = false;     // 2D 게임이므로 강제
        _monster.target = _playerTransform;     // 타겟 없으면 안 움직임
        _monster.SyncTransformToPhysics();

        _registry.Register(_monster.gameObject);
    }

    private Vector2 GetSpawnPositionByPattern(bool _isBoss)
    {
        if (_isBoss)
        {
            // 보스는 플레이어 기준 가까운 곳(연출 포인트 생기면 교체)
            return (Vector2)_playerTransform.position + Vector2.up * 8f;
        }

        if (_currentWave == null)
            return GetRandomRingPosition();

        switch (_currentWave.spawnPattern)
        {
            case SpawnPattern.Random:
                return GetRandomRingPosition();

            case SpawnPattern.Squad:
                return GetSquadPosition();

            case SpawnPattern.Circle:
                return GetCirclePosition();

            default:
                return GetRandomRingPosition();
        }
    }

    private Vector2 GetRandomRingPosition()
    {
        Vector2 _dir = Random.insideUnitCircle.normalized;
        float _dist = Random.Range(MIN_SPAWN_DISTANCE, MAX_SPAWN_DISTANCE);
        return (Vector2)_playerTransform.position + _dir * _dist;
    }

    private Vector2 GetSquadPosition()
    {
        Vector2 _offset = Random.insideUnitCircle * SQUAD_RADIUS;
        return _squadCenter + _offset;
    }

    private Vector2 GetCirclePosition()
    {
        float _angle = (360f / CIRCLE_SLOTS) * (_circleIndex % CIRCLE_SLOTS);
        _circleIndex++;

        float _rad = _angle * Mathf.Deg2Rad;
        float _dist = Random.Range(MIN_SPAWN_DISTANCE, MAX_SPAWN_DISTANCE);

        Vector2 _offset = new Vector2(Mathf.Cos(_rad), Mathf.Sin(_rad)) * _dist;
        return (Vector2)_playerTransform.position + _offset;
    }

    private int PickMonsterIdByWeight(WavePlan _wave)
    {
        if (_wave == null || _wave.spawns == null || _wave.spawns.Count == 0)
            return 0;

        int _total = 0;
        for (int i = 0; i < _wave.spawns.Count; i++)
        {
            int _w = _wave.spawns[i].weightPercent;
            if (_w <= 0) _w = 100;
            _total += _w;
        }

        int _roll = Random.Range(1, _total + 1);
        int _acc = 0;

        for (int i = 0; i < _wave.spawns.Count; i++)
        {
            int _w = _wave.spawns[i].weightPercent;
            if (_w <= 0) _w = 100;
            _acc += _w;

            if (_roll <= _acc)
                return _wave.spawns[i].monsterId;
        }

        return _wave.spawns[0].monsterId;
    }
}
