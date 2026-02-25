using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private MonsterBase _specialMonsterPrefab;
    [SerializeField] private MonsterBase _midBossPrefab;
    [SerializeField] private MonsterBase _bossPrefab;

    private WavePlan _currentWave;

    private bool _isSpawningEnabled = true; //스폰 활성화 여부
    private float _spawnAccmulator; //스폰 타이밍 누적값

    private List<SpecialMonsterRow> _specialRows;
    private int _currentMainSeconds;
    private int _lastSpecialTickSeconds = -1;

    // periodic 중복 방지용(각 row별로 마지막 소환 초 기록)
    private Dictionary<int, int> _lastPeriodicSpawnSecondBySpecialId = new Dictionary<int, int>();
    private HashSet<int> _onceSpawnedSpecialIds = new HashSet<int>();

    // 중간보스 및 보스의 경우, 스폰 시점에 참조할 수 있도록 마지막으로 소환된 객체 저장
    public GameObject LastSpawnedBoss { get; private set; }
    public GameObject LastSpawnedMidBoss { get; private set; }

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
                break;

            SpawnOneFromWave();
            TrySpawnSpecialByTime();
        }

    }

    public void SpawnBoss(int _bossId)
    {
        if (_bossPrefab == null) return;

        MonsterBase boss = SpawnPrefab(_bossPrefab, $"Boss_{_bossId}", _isBoss: true);
        LastSpawnedBoss = boss != null ? boss.gameObject : null;
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

        MonsterBase mid = SpawnPrefab(_midBossPrefab, $"MidBoss_{_midBossId}", _isBoss: true);
        LastSpawnedMidBoss = mid != null ? mid.gameObject : null;
    }

    private void SpawnOneFromWave()
    {
        int _monsterId = PickMonsterIdByWeight(_currentWave);

        int amount = 1;
        if (_currentWave.spawns != null && _currentWave.spawns.Count > 0)
            amount = Mathf.Max(1, _currentWave.spawns[0].spawnAmount); // 간단히 첫 row 기준

        for (int i = 0; i < amount; i++)
        {
            if (!_registry.CanSpawnMore()) break;
            SpawnPrefab(_normalMonsterPrefab, $"Monster_{_monsterId}", _isBoss: false);
        }
    }

    private MonsterBase SpawnPrefab(MonsterBase _prefab, string _name, bool _isBoss)
    {
        Vector2 _pos2D = GetSpawnPositionByPattern(_isBoss);
        Vector3 _spawnPos = new Vector3(_pos2D.x, _pos2D.y, 0f);

        MonsterBase _monster = Instantiate(_prefab, _spawnPos, Quaternion.identity);
        _monster.name = _name;

        _monster.target = _playerTransform;

        _registry.Register(_monster.gameObject);
        return _monster;
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

    public void SetMainSeconds(int _mainSeconds)
    {
        _currentMainSeconds = _mainSeconds;
    }

    public void ApplySpecialGroup(List<SpecialMonsterRow> _rows)
    {
        _specialRows = _rows;

        _lastSpecialTickSeconds = -1;
        _lastPeriodicSpawnSecondBySpecialId.Clear();
        _onceSpawnedSpecialIds.Clear();
    }

    private void TrySpawnSpecialByTime()
    {
        if (_specialRows == null || _specialRows.Count == 0) return;
        if (_specialMonsterPrefab == null) return;

        if (_currentMainSeconds == _lastSpecialTickSeconds) return;
        _lastSpecialTickSeconds = _currentMainSeconds;

        for (int i = 0; i < _specialRows.Count; i++)
        {
            SpecialMonsterRow row = _specialRows[i];

            if (row.monster_id < 0) continue;

            // Once: start_time 딱 1회
            if (row.spawn_type == (int)SpecialSpawnType.Once)
            {
                if (_currentMainSeconds != row.start_time) continue;

                if (_onceSpawnedSpecialIds.Contains(row.special_id)) continue;
                _onceSpawnedSpecialIds.Add(row.special_id);

                SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false);
                continue;
            }

            // Periodic: start~end 구간 동안 generation_time 주기
            if (row.spawn_type == (int)SpecialSpawnType.Periodic)
            {
                if (_currentMainSeconds < row.start_time || _currentMainSeconds > row.end_time) continue;

                int interval = Mathf.Max(1, row.generation_time);

                int lastSec;
                bool hasLast = _lastPeriodicSpawnSecondBySpecialId.TryGetValue(row.special_id, out lastSec);

                // start_time에 첫 스폰 (원하면 제거 가능)
                if (!hasLast)
                {
                    _lastPeriodicSpawnSecondBySpecialId[row.special_id] = _currentMainSeconds;
                    SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false);
                    continue;
                }

                if (_currentMainSeconds - lastSec >= interval)
                {
                    _lastPeriodicSpawnSecondBySpecialId[row.special_id] = _currentMainSeconds;
                    SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false);
                }
            }
        }
    }
}
