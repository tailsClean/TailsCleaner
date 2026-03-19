using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTriggerPatternRunner : MonoBehaviour
{
    private const string MONSTER_TABLE_FILE = "monster_table";
    private const string PATTERN_GROUP_TABLE_FILE = "pattern_group";
    private const string PATTERN_GROUP_COMPOSITION_TABLE_FILE = "pattern_group_composition";
    private const string PATTERN_TABLE_FILE = "pattern";

    [Header("공통 기본값")]
    [SerializeField] private float _defaultWarningDuration = 2f;
    [SerializeField] private float _defaultExpAbsorbDelay = 5f;
    [SerializeField] private float _defaultExpAbsorbDuration = 3f;
    [SerializeField] private float _defaultDirtySpawnDuration = 5f;
    [SerializeField] private float _defaultDirtyAbsorbDuration = 2f;

    [Header("더러움 소환용 프리팹")]
    [SerializeField] private PoolObject _bossDirtyItemPrefab;

    private MonsterBase _boss;
    private StageController _stageController;
    private StageTimer _timer;

    private PatternGroupTableRow _patternGroupRow;
    private readonly List<PatternGroupCompositionTableRow> _compositionRows = new List<PatternGroupCompositionTableRow>();
    private readonly List<PatternTableRow> _triggerRows = new List<PatternTableRow>();

    private bool _bound;
    private bool _patternRunning;

    private readonly HashSet<int> _triggeredOncePatternIds = new HashSet<int>();
    private readonly Dictionary<int, int> _enrageCurrentStepByPatternId = new Dictionary<int, int>();

    private Coroutine _runningRoutine;

    public void Bind(MonsterBase boss)
    {
        Unbind();

        _boss = boss;
        _stageController = StageController.Instance;

        if (_stageController == null)
            _stageController = FindFirstObjectByType<StageController>();

        if (_boss == null || _stageController == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] bind 실패");
            return;
        }

        _timer = _stageController.Timer;
        if (_timer == null || _stageController.Events == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] timer/events 없음");
            return;
        }

        LoadTriggerRows();

        if (_triggerRows.Count == 0)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] trigger row 없음 / monsterId={GetMonsterId()}");
            return;
        }

        ResetState();

        _stageController.Events.OnBossSecondTick += HandleBossSecondTick;
        _bound = true;

        Debug.Log($"[BossTriggerPatternRunner] Bind 성공 / triggerCount={_triggerRows.Count}");
    }

    public void Unbind()
    {
        if (_bound && _stageController != null && _stageController.Events != null)
            _stageController.Events.OnBossSecondTick -= HandleBossSecondTick;

        if (_runningRoutine != null)
            StopCoroutine(_runningRoutine);

        if (_boss != null)
            _boss.SetAttackingState(false);

        _patternRunning = false;
        _runningRoutine = null;
        _bound = false;

        _patternGroupRow = null;
        _compositionRows.Clear();
        _triggerRows.Clear();
        _triggeredOncePatternIds.Clear();
        _enrageCurrentStepByPatternId.Clear();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void ResetState()
    {
        _patternRunning = false;
        _triggeredOncePatternIds.Clear();
        _enrageCurrentStepByPatternId.Clear();

        for (int i = 0; i < _triggerRows.Count; i++)
        {
            int patternId = _triggerRows[i].pattern_id;
            _enrageCurrentStepByPatternId[patternId] = 0;
        }
    }

    private void LoadTriggerRows()
    {
        _triggerRows.Clear();
        _compositionRows.Clear();
        _patternGroupRow = null;

        int monsterId = GetMonsterId();
        if (monsterId <= 0)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] invalid monsterId={monsterId}");
            return;
        }

        List<MonsterTableRow> monsterRows = DataParser.Parse<MonsterTableRow>(MONSTER_TABLE_FILE);
        List<PatternGroupTableRow> patternGroupRows = DataParser.Parse<PatternGroupTableRow>(PATTERN_GROUP_TABLE_FILE);
        List<PatternGroupCompositionTableRow> compositionRows =
            DataParser.Parse<PatternGroupCompositionTableRow>(PATTERN_GROUP_COMPOSITION_TABLE_FILE);
        List<PatternTableRow> patternRows = DataParser.Parse<PatternTableRow>(PATTERN_TABLE_FILE);

        if (monsterRows == null || patternGroupRows == null || compositionRows == null || patternRows == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] CSV 로드 실패");
            return;
        }

        MonsterTableRow monsterRow = null;
        for (int i = 0; i < monsterRows.Count; i++)
        {
            if (monsterRows[i].monster_id == monsterId)
            {
                monsterRow = monsterRows[i];
                break;
            }
        }

        if (monsterRow == null)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] monsterRow 없음 / monsterId={monsterId}");
            return;
        }

        int patternGroupId = monsterRow.pattern_group_id;

        for (int i = 0; i < patternGroupRows.Count; i++)
        {
            if (patternGroupRows[i].pattern_group_id == patternGroupId)
            {
                _patternGroupRow = patternGroupRows[i];
                break;
            }
        }

        if (_patternGroupRow == null)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] pattern_group 없음 / groupId={patternGroupId}");
            return;
        }

        for (int i = 0; i < compositionRows.Count; i++)
        {
            if (compositionRows[i].pattern_group_id == patternGroupId)
                _compositionRows.Add(compositionRows[i]);
        }

        if (_compositionRows.Count == 0)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] composition 없음 / groupId={patternGroupId}");
            return;
        }

        _compositionRows.Sort((a, b) => a.priority.CompareTo(b.priority));

        for (int i = 0; i < _compositionRows.Count; i++)
        {
            int patternId = _compositionRows[i].pattern_id;

            for (int j = 0; j < patternRows.Count; j++)
            {
                if (patternRows[j].pattern_id == patternId)
                {
                    if (IsTriggerPattern(patternRows[j].pattern_logic_type))
                        _triggerRows.Add(patternRows[j]);

                    break;
                }
            }
        }
    }

    private bool IsTriggerPattern(string logicType)
    {
        if (string.IsNullOrWhiteSpace(logicType))
            return false;

        logicType = logicType.Trim().ToLower();

        return logicType == "trigger_exp_absorb"
            || logicType == "trigger_dirty_spawn"
            || logicType == "trigger_enrage";
    }

    private int GetMonsterId()
    {
        if (_boss == null) return -1;
        return _boss.MonsterId;
    }

    private void HandleBossSecondTick(int secondsLeft)
    {
        if (_timer == null) return;
        if (_boss == null) return;
        if (_boss.hp <= 0f) return;
        if (_patternRunning) return;

        int elapsed = _timer.GetBossLimitSeconds() - secondsLeft;

        for (int i = 0; i < _triggerRows.Count; i++)
        {
            PatternTableRow row = _triggerRows[i];
            string logicType = row.pattern_logic_type?.Trim().ToLower();

            switch (logicType)
            {
                case "trigger_exp_absorb":
                    TryTriggerExpAbsorb(row, elapsed);
                    break;

                case "trigger_dirty_spawn":
                    TryTriggerDirtySpawn(row, elapsed);
                    break;

                case "trigger_enrage":
                    TryTriggerEnrage(row, elapsed);
                    break;
            }

            if (_patternRunning)
                break;
        }
    }

    private void TryTriggerExpAbsorb(PatternTableRow row, int elapsed)
    {
        if (_triggeredOncePatternIds.Contains(row.pattern_id)) return;

        int triggerTime = row.cast_time > 0f
            ? Mathf.RoundToInt(row.cast_time)
            : Mathf.RoundToInt(_defaultExpAbsorbDelay);

        if (elapsed < triggerTime) return;

        _triggeredOncePatternIds.Add(row.pattern_id);
        _runningRoutine = StartCoroutine(CoExpAbsorb(row));
    }

    private void TryTriggerDirtySpawn(PatternTableRow row, int elapsed)
    {
        if (_triggeredOncePatternIds.Contains(row.pattern_id)) return;

        int triggerTime = row.cast_time > 0f ? Mathf.RoundToInt(row.cast_time) : 10;
        if (elapsed < triggerTime) return;

        _triggeredOncePatternIds.Add(row.pattern_id);
        _runningRoutine = StartCoroutine(CoDirtySpawn(row));
    }

    private void TryTriggerEnrage(PatternTableRow row, int elapsed)
    {
        if (row.enrage_max_step <= 0) return;

        int currentStep = _enrageCurrentStepByPatternId[row.pattern_id];
        if (currentStep >= row.enrage_max_step) return;

        int interval = Mathf.RoundToInt(row.enrage_time);
        if (interval <= 0) return;

        int nextTriggerTime = interval * (currentStep + 1);
        if (elapsed < nextTriggerTime) return;

        _runningRoutine = StartCoroutine(CoEnrage(row));
    }

    private IEnumerator CoExpAbsorb(PatternTableRow row)
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        float warning = row.cast_time > 0f ? row.cast_time : _defaultWarningDuration;
        yield return new WaitForSeconds(warning);

        List<InGameExpItem> targets = InGameExpItem.GetAllUncleaned();
        float absorbDuration = row.duration > 0f ? row.duration : _defaultExpAbsorbDuration;

        int absorbedCount = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                StartCoroutine(targets[i].MoveToBossAndAbsorb(_boss.transform, absorbDuration, () =>
                {
                    absorbedCount++;
                }));
            }
        }

        yield return new WaitForSeconds(absorbDuration + 0.05f);

        _boss.hp += absorbedCount * row.dirty_to_hp_value;

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    private IEnumerator CoDirtySpawn(PatternTableRow row)
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        float warning = row.cast_time > 0f ? row.cast_time : _defaultWarningDuration;
        yield return new WaitForSeconds(warning);

        SpawnDirtyItems(row);

        float dirtyDuration = row.duration > 0f ? row.duration : _defaultDirtySpawnDuration;
        yield return new WaitForSeconds(dirtyDuration);

        List<InGameExpItem> targets = InGameExpItem.GetBossSpawnedUncleaned();

        int absorbedCount = 0;

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                StartCoroutine(targets[i].MoveToBossAndAbsorb(_boss.transform, _defaultDirtyAbsorbDuration, () =>
                {
                    absorbedCount++;
                }));
            }
        }

        yield return new WaitForSeconds(_defaultDirtyAbsorbDuration + 0.05f);

        _boss.hp += absorbedCount * row.dirty_to_hp_value;

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    private IEnumerator CoEnrage(PatternTableRow row)
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        yield return new WaitForSeconds(_defaultWarningDuration);

        _enrageCurrentStepByPatternId[row.pattern_id]++;

        _boss.hp *= Mathf.Max(1f, row.enrage_hp_rate);
        _boss.power *= Mathf.Max(1f, row.enrage_atk_rate);

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    private void SpawnDirtyItems(PatternTableRow row)
    {
        if (_bossDirtyItemPrefab == null || ObjectPoolManager.Instance == null)
            return;

        Transform player = _boss.target;
        List<Vector3> usedPositions = new List<Vector3>();

        int spawnCount = row.summon_count > 0 ? row.summon_count : 5;
        float radius = row.summon_radius > 0f ? row.summon_radius : 3f;

        for (int i = 0; i < spawnCount; i++)
        {
            bool spawnNearBoss = row.summon_position_type == 1 || (row.summon_position_type != 0 && i % 2 == 0);

            Vector3 center = spawnNearBoss
                ? _boss.transform.position
                : (player != null ? player.position : _boss.transform.position);

            Vector3 spawnPos = FindNonOverlappingPosition(center, radius, usedPositions);

            PoolObject spawned = ObjectPoolManager.Instance.Spawn(_bossDirtyItemPrefab, spawnPos, Quaternion.identity);

            if (spawned != null && spawned.TryGetComponent<InGameExpItem>(out var expItem))
            {
                expItem.SetExp(0);
                expItem.SetBossSpawnedDirt(true);
            }

            usedPositions.Add(spawnPos);
        }
    }

    private Vector3 FindNonOverlappingPosition(Vector3 center, float radius, List<Vector3> usedPositions)
    {
        const int maxTry = 20;
        const float minDistance = 0.75f;

        for (int i = 0; i < maxTry; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;
            Vector3 candidate = center + new Vector3(offset.x, offset.y, 0f);

            bool overlapped = false;
            for (int j = 0; j < usedPositions.Count; j++)
            {
                if (Vector3.Distance(candidate, usedPositions[j]) < minDistance)
                {
                    overlapped = true;
                    break;
                }
            }

            if (!overlapped)
                return candidate;
        }

        return center;
    }
}