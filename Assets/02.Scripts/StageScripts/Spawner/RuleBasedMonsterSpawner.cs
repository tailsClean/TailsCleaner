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
    
    [Header("Spawn Bounds")]
    [SerializeField] private float _wallMargin = 1.5f; // 벽에서 떨어질 거리
    [SerializeField] private int _maxPositionTry = 10; // 위치 재시도 횟수

    [Header("Prefabs")]
    [SerializeField] private MonsterBase _normalMonsterPrefab;
    [SerializeField] private MonsterBase _specialMonsterPrefab;
    [SerializeField] private MonsterBase _midBossPrefab;
    [SerializeField] private MonsterBase _bossPrefab;

    //스테이지 및 웨이브 상승 시, 공격력 및 체력 증가를 위한 변수값
    private float _stageHpMod;
    private float _stagePowerMod;
    private float _towerHpMod;
    private float _towerPowerMod;

    private WavePlan _currentWave;

    private bool _isSpawningEnabled = true;
    private float _spawnAccmulator;

    private List<SpecialMonsterRow> _specialRows;
    private int _currentMainSeconds;
    private int _lastSpecialTickSeconds = -1;

    private Dictionary<int, int> _lastPeriodicSpawnSecondBySpecialId = new Dictionary<int, int>();
    private HashSet<int> _onceSpawnedSpecialIds = new HashSet<int>();

    public GameObject LastSpawnedBoss { get; private set; }
    public GameObject LastSpawnedMidBoss { get; private set; }

    private Vector2 _squadCenter;
    private int _circleIndex;

    private bool _expCacheReady;
    private Dictionary<int, int> _monsterIdToType;
    private Dictionary<int, float> _typeToBaseExp;

    private Bounds _spawnBounds;
    private bool _hasBounds = false;

    private const string MONSTER_TABLE_FILE = "monster/monster";
    private const string MONSTER_TYPE_TABLE_FILE = "monster/monster_type";

    public void SetSpawnBounds(Bounds bounds)
    {
        _spawnBounds = bounds;
        _hasBounds = true;

        Debug.Log($"[Spawner] Bounds set: min={bounds.min}, max={bounds.max}");
    }

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
        if (!_isSpawningEnabled) return;
        if (_playerTransform == null || _registry == null) return;
        if (_normalMonsterPrefab == null) return;
        if (_currentWave == null) return;
        if (!_registry.CanSpawnMore()) return;

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

        MonsterBase boss = SpawnPrefab(_bossPrefab, $"Boss_{_bossId}", _isBoss: true, _bossId);
        LastSpawnedBoss = boss != null ? boss.gameObject : null;
    }

    public void SpawnMidBoss(int _midBossId)
    {
        if (_midBossId == NO_BOSS_ID) return;
        if (_midBossPrefab == null) return;

        MonsterBase mid = SpawnPrefab(_midBossPrefab, $"MidBoss_{_midBossId}", _isBoss: true, _midBossId);
        LastSpawnedMidBoss = mid != null ? mid.gameObject : null;
    }

    private void SpawnOneFromWave()
    {
        int monsterId = PickMonsterIdBySpawnAmount(_currentWave);
        if (monsterId <= 0) return;

        SpawnPrefab(_normalMonsterPrefab, $"Monster_{monsterId}", _isBoss: false, monsterId);
    }

    private void EnsureExpCache()
    {
        if (_expCacheReady) return;

        var monsterRows = DataParser.Parse<MonsterTableRow>(MONSTER_TABLE_FILE);
        var typeRows = DataParser.Parse<MonsterTypeTableRow>(MONSTER_TYPE_TABLE_FILE);

        if (monsterRows == null || typeRows == null)
        {
            Debug.LogError($"[ExpCache] Parse failed. monsterRowsNull={monsterRows == null}, typeRowsNull={typeRows == null}");
            return;
        }

        _monsterIdToType = new Dictionary<int, int>(monsterRows.Count);
        _typeToBaseExp = new Dictionary<int, float>(typeRows.Count);

        for (int i = 0; i < monsterRows.Count; i++)
            _monsterIdToType[monsterRows[i].monster_id] = monsterRows[i].monster_type;

        for (int i = 0; i < typeRows.Count; i++)
            _typeToBaseExp[typeRows[i].monster_type] = typeRows[i].base_exp;

        _expCacheReady = true;
        Debug.Log($"[ExpCache] Ready. monsterMap={_monsterIdToType.Count}, typeMap={_typeToBaseExp.Count}");
    }

    private int CalcExp(int monsterId)
    {
        EnsureExpCache();

        int type = 0;
        if (_monsterIdToType != null && _monsterIdToType.TryGetValue(monsterId, out var t))
            type = t;

        float baseExp = 0f;
        if (_typeToBaseExp != null && _typeToBaseExp.TryGetValue(type, out var b))
            baseExp = b;

        float mul = _currentWave != null ? _currentWave.waveExpMultiply : 0f;
        mul = Mathf.Max(0f, mul);

        return Mathf.RoundToInt(baseExp * (1f + mul));
    }

    private MonsterBase SpawnPrefab(MonsterBase _prefab, string _name, bool _isBoss, int _monsterId)
    {
        Vector2 _pos2D = GetSpawnPositionByPattern(_isBoss);
        Vector3 _spawnPos = new Vector3(_pos2D.x, _pos2D.y, 0f);

        MonsterBase _monster;
        if (ObjectPoolManager.Instance != null)
            _monster = ObjectPoolManager.Instance.Spawn(_prefab, _spawnPos, Quaternion.identity);
        else
            _monster = Instantiate(_prefab, _spawnPos, Quaternion.identity);

        if (_monster == null)
        {
            Debug.LogError($"[Spawner] Spawn failed. name={_name}, monsterId={_monsterId}");
            return null;
        }

        Debug.Log($"[Spawner] SpawnPrefab / name={_name}, monsterId={_monsterId}, type={_monster.GetType().Name}");

        _monster.name = _name;
        _monster.SetMonsterId(_monsterId);
        _monster.target = _playerTransform;

        float waveHp = _currentWave != null ? _currentWave.waveHpModifier : 0f;
        float wavePower = _currentWave != null ? _currentWave.wavePowerModifier : 0f;

        float hpScale = 1f + _towerHpMod + _stageHpMod + waveHp;
        float powerScale = 1f + _towerPowerMod + _stagePowerMod + wavePower;

        hpScale = Mathf.Max(0.1f, hpScale);
        powerScale = Mathf.Max(0.1f, powerScale);

        _monster.ApplyScaling(hpScale, powerScale);

        int exp = CalcExp(_monsterId);
        _monster.SetExpReward(exp);

        BossTriggerPatternRunner runner = _monster.GetComponent<BossTriggerPatternRunner>();
        if (runner != null)
        {
            runner.Bind(_monster);
        }

        _registry.Register(_monster.gameObject);
        return _monster;
    }

    public void SetStageModifiers(float stageHp, float stagePower, float towerHp, float towerPower)
    {
        _stageHpMod = stageHp;
        _stagePowerMod = stagePower;
        _towerHpMod = towerHp;
        _towerPowerMod = towerPower;
    }

    private Vector2 GetSpawnPositionByPattern(bool _isBoss)
    {
        if (_isBoss)
        {
            return ClampToBounds((Vector2)_playerTransform.position + Vector2.up * 8f);
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

    private Vector2 ClampToBounds(Vector2 pos)
    {
        if (!_hasBounds) return pos;

        return new Vector2(
            Mathf.Clamp(pos.x, _spawnBounds.min.x + _wallMargin, _spawnBounds.max.x - _wallMargin),
            Mathf.Clamp(pos.y, _spawnBounds.min.y + _wallMargin, _spawnBounds.max.y - _wallMargin)
        );
    }

    private bool IsInsideBounds(Vector2 pos)
    {
        if (!_hasBounds) return true;

        return pos.x >= _spawnBounds.min.x + _wallMargin &&
               pos.x <= _spawnBounds.max.x - _wallMargin &&
               pos.y >= _spawnBounds.min.y + _wallMargin &&
               pos.y <= _spawnBounds.max.y - _wallMargin;
    }

    private Vector2 GetRandomRingPosition()
    {
        for (int i = 0; i < _maxPositionTry; i++)
        {
            Vector2 _dir = Random.insideUnitCircle.normalized;
            float _dist = Random.Range(MIN_SPAWN_DISTANCE, MAX_SPAWN_DISTANCE);
            Vector2 candidate = (Vector2)_playerTransform.position + _dir * _dist;

            if (!IsInsideBounds(candidate))
                continue;

            return ClampToBounds(candidate);
        }

        return ClampToBounds((Vector2)_playerTransform.position + Vector2.right * MIN_SPAWN_DISTANCE);
    }

    private Vector2 GetSquadPosition()
    {
        for (int i = 0; i < _maxPositionTry; i++)
        {
            Vector2 _offset = Random.insideUnitCircle * SQUAD_RADIUS;
            Vector2 candidate = _squadCenter + _offset;

            if (!IsInsideBounds(candidate))
                continue;

            Debug.Log($"[SquadSpawn] squadCenter={_squadCenter}, candidate={candidate}, try={i}");
            return ClampToBounds(candidate);
        }

        Debug.LogWarning($"[SquadSpawn] fallback squadCenter used. squadCenter={_squadCenter}");
        return ClampToBounds(_squadCenter);
    }

    private Vector2 GetCirclePosition()
    {
        for (int tryCount = 0; tryCount < _maxPositionTry; tryCount++)
        {
            float _angle = (360f / CIRCLE_SLOTS) * (_circleIndex % CIRCLE_SLOTS);
            _circleIndex++;

            float _rad = _angle * Mathf.Deg2Rad;
            float _dist = Random.Range(MIN_SPAWN_DISTANCE, MAX_SPAWN_DISTANCE);

            Vector2 _offset = new Vector2(Mathf.Cos(_rad), Mathf.Sin(_rad)) * _dist;
            Vector2 candidate = (Vector2)_playerTransform.position + _offset;

            if (!IsInsideBounds(candidate))
                continue;

            return ClampToBounds(candidate);
        }

        return ClampToBounds((Vector2)_playerTransform.position + Vector2.up * MIN_SPAWN_DISTANCE);
    }

    private int PickMonsterIdBySpawnAmount(WavePlan _wave)
    {
        if (_wave == null || _wave.spawns == null || _wave.spawns.Count == 0)
            return 0;

        int _total = 0;
        for (int i = 0; i < _wave.spawns.Count; i++)
        {
            int w = Mathf.Max(0, _wave.spawns[i].spawnAmount);
            _total += w;
        }

        if (_total <= 0)
            return _wave.spawns[0].monsterId;

        int _roll = Random.Range(1, _total + 1);
        int _acc = 0;

        for (int i = 0; i < _wave.spawns.Count; i++)
        {
            int w = Mathf.Max(0, _wave.spawns[i].spawnAmount);
            _acc += w;
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

            if (row.spawn_type == (int)SpecialSpawnType.Once)
            {
                if (_currentMainSeconds != row.start_time) continue;

                if (_onceSpawnedSpecialIds.Contains(row.special_id)) continue;
                _onceSpawnedSpecialIds.Add(row.special_id);

                SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false, row.monster_id);
                continue;
            }

            if (row.spawn_type == (int)SpecialSpawnType.Periodic)
            {
                if (_currentMainSeconds < row.start_time || _currentMainSeconds > row.end_time) continue;

                int interval = Mathf.Max(1, row.generation_time);

                int lastSec;
                bool hasLast = _lastPeriodicSpawnSecondBySpecialId.TryGetValue(row.special_id, out lastSec);

                if (!hasLast)
                {
                    _lastPeriodicSpawnSecondBySpecialId[row.special_id] = _currentMainSeconds;
                    SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false, row.monster_id);
                    continue;
                }

                if (_currentMainSeconds - lastSec >= interval)
                {
                    _lastPeriodicSpawnSecondBySpecialId[row.special_id] = _currentMainSeconds;
                    SpawnPrefab(_specialMonsterPrefab, $"Special_{row.monster_id}", _isBoss: false, row.monster_id);
                }
            }
        }
    }
}