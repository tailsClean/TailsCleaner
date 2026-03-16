using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossTriggerPatternRunner : MonoBehaviour
{
    private const string BOSS_TRIGGER_PATTERN_TABLE_FILE = "boss_trigger_pattern";

    [Header("Shared")]
    [SerializeField] private float _warningDuration = 2f;

    [Header("Exp Absorb")]
    [SerializeField] private float _expAbsorbDelay = 5f;
    [SerializeField] private float _expAbsorbDuration = 3f;

    [Header("Dirty Spawn")]
    [SerializeField] private PoolObject _bossDirtyItemPrefab;
    [SerializeField] private float _spawnRadiusFromBoss = 3f;
    [SerializeField] private float _spawnRadiusFromPlayer = 3f;
    [SerializeField] private float _dirtySpawnFirstDelay = 10f;
    [SerializeField] private float _dirtySpawnDuration = 5f;
    [SerializeField] private float _dirtyAbsorbDuration = 2f;
    [SerializeField] private int _dirtySpawnCount = 5;

    private SpecialBossMonsterBase _boss;
    private StageController _stageController;
    private StageTimer _timer;

    private BossTriggerPatternRow _row;

    private bool _bound;
    private bool _patternRunning;

    private bool _expTriggered;
    private int _enrageCurrentStep;

    private Coroutine _runningRoutine;

    public void Bind(SpecialBossMonsterBase boss)
    {
        Unbind();

        _boss = boss;
        _stageController = StageController.Instance;

        if (_boss == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] 보스 없음");
            return;
        }

        if (_stageController == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] 스테이지 컨트롤러 없음");
            return;
        }

        _timer = _stageController.Timer;
        if (_timer == null || _stageController.Events == null)
        {
            Debug.LogWarning("[BossTriggerPatternRunner] 타이머 혹은 이벤트 없음");
            return;
        }

        LoadRow();

        if (_row == null)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] Row값을 찾을 수 없음. monsterId={GetMonsterId()}");
            return;
        }

        ResetState();

        _stageController.Events.OnBossSecondTick += HandleBossSecondTick;
        _bound = true;
    }

    public void Unbind()
    {
        if (_bound && _stageController != null && _stageController.Events != null)
        {
            _stageController.Events.OnBossSecondTick -= HandleBossSecondTick;
        }

        if (_runningRoutine != null && _stageController != null)
        {
            _stageController.StopCoroutine(_runningRoutine);
        }

        _runningRoutine = null;
        _bound = false;
    }

    private void OnDestroy()
    {
        Unbind();
    }

    private void ResetState()
    {
        _patternRunning = false;
        _expTriggered = false;
        _enrageCurrentStep = 0;
    }

    private void LoadRow()
    {
        int monsterId = GetMonsterId();
        if (monsterId <= 0)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] 몬스터 좌표 0 미만 : {monsterId}");
            _row = null;
            return;
        }

        List<BossTriggerPatternRow> rows = DataParser.Parse<BossTriggerPatternRow>(BOSS_TRIGGER_PATTERN_TABLE_FILE);
        if (rows == null || rows.Count == 0)
        {
            Debug.LogWarning($"[BossTriggerPatternRunner] csv 로드 안됨 : {BOSS_TRIGGER_PATTERN_TABLE_FILE}");
            _row = null;
            return;
        }

        _row = null;

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].monster_id == monsterId)
            {
                _row = rows[i];
                break;
            }
        }
    }

    private int GetMonsterId()
    {
        if (_boss == null) return -1;

        string objName = _boss.gameObject.name;
        if (string.IsNullOrEmpty(objName))
            return -1;

        int underscoreIndex = objName.LastIndexOf('_');
        if (underscoreIndex < 0 || underscoreIndex >= objName.Length - 1)
            return -1;

        string idText = objName.Substring(underscoreIndex + 1);

        if (int.TryParse(idText, out int monsterId))
            return monsterId;

        return -1;
    }

    private void HandleBossSecondTick(int secondsLeft)
    {
        if (_row == null) return;
        if (_timer == null) return;
        if (_patternRunning) return;
        if (_boss == null) return;
        if (_boss.hp <= 0f) return;

        int elapsed = _timer.GetBossLimitSeconds() - secondsLeft;

        switch (_row.pattern_logic_type)
        {
            case "trigger_exp_absorb":
                TryTriggerExpAbsorb(elapsed);
                break;

            case "trigger_dirty_spawn":
                TryTriggerDirtySpawn(elapsed);
                break;

            case "trigger_enrage":
                TryTriggerEnrage(elapsed);
                break;
        }
    }

    // 경험치 방치 패턴
    private void TryTriggerExpAbsorb(int elapsed)
    {
        if (_expTriggered) return;
        if (elapsed < Mathf.RoundToInt(_expAbsorbDelay)) return;

        _expTriggered = true;
        _runningRoutine = _stageController.StartCoroutine(CoExpAbsorb());
    }


    // 더러움 소환 패턴
    // 반복 여부는 지금 기획/CSV 기준 불명확해서 1회 실행으로 둔다.
    private void TryTriggerDirtySpawn(int elapsed)
    {
        if (_expTriggered) { } // no-op, 가독성용

        if (elapsed < Mathf.RoundToInt(_dirtySpawnFirstDelay)) return;

        // 현재 CSV에는 dirty spawn 반복 주기 컬럼이 없어서 1회 실행 처리
        if (_runningRoutine != null || _patternRunning) return;

        // 보스 소환 더러움 패턴도 한 번만 실행
        // 필요 시 CSV 컬럼 추가 후 반복형으로 확장 가능
        _runningRoutine = _stageController.StartCoroutine(CoDirtySpawnOnce());
    }


    // 광폭화는 CSV의 enrage_time을 간격으로 사용한다.
    // enrage_time=60, max_step=2 -> 60초/120초에 발동
    private void TryTriggerEnrage(int elapsed)
    {
        if (_row.enrage_max_step <= 0) return;
        if (_enrageCurrentStep >= _row.enrage_max_step) return;

        int interval = Mathf.RoundToInt(_row.enrage_time);
        if (interval <= 0) return;

        int nextTriggerTime = interval * (_enrageCurrentStep + 1);
        if (elapsed < nextTriggerTime) return;

        _runningRoutine = _stageController.StartCoroutine(CoEnrage());
    }

    // 경험치 방치:
    // 예고 -> 필드의 미청소 더러움 흡수 -> 개수 비례 hp 회복 -> 종료
    private IEnumerator CoExpAbsorb()
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        // TODO: 경험치 방치 예고 연출
        yield return new WaitForSeconds(_warningDuration);

        List<InGameExpItem> targets = InGameExpItem.GetAllUncleaned();

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                _stageController.StartCoroutine(
                    targets[i].MoveToBossAndAbsorb(_boss.transform, _expAbsorbDuration));
            }
        }

        yield return new WaitForSeconds(_expAbsorbDuration);

        float hpGain = targets.Count * _row.dirty_to_hp_value;
        _boss.hp += hpGain;

        // TODO: 경험치 방치 공격 이펙트

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    // 더러움 소환:
    // 예고 -> 더러움 생성 -> 유지 -> 남은 더러움 흡수 -> 개수 비례 hp 회복 -> 종료
    private IEnumerator CoDirtySpawnOnce()
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        // TODO: 더러움 소환 예고 연출
        yield return new WaitForSeconds(_warningDuration);

        SpawnDirtyItems();

        yield return new WaitForSeconds(_dirtySpawnDuration);

        List<InGameExpItem> targets = InGameExpItem.GetBossSpawnedUncleaned();

        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null)
            {
                _stageController.StartCoroutine(
                    targets[i].MoveToBossAndAbsorb(_boss.transform, _dirtyAbsorbDuration));
            }
        }

        yield return new WaitForSeconds(_dirtyAbsorbDuration);

        float hpGain = targets.Count * _row.dirty_to_hp_value;
        _boss.hp += hpGain;

        // TODO: 더러움 소환 공격 이펙트

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    // 광폭화:
    // 예고 -> 단계 증가 -> CSV 배율만큼 hp/power 증가 -> 종료
    private IEnumerator CoEnrage()
    {
        _patternRunning = true;
        _boss.SetAttackingState(true);

        // TODO: 광폭화 예고 연출
        yield return new WaitForSeconds(_warningDuration);

        _enrageCurrentStep++;

        _boss.hp *= Mathf.Max(1f, _row.enrage_hp_rate);
        _boss.power *= Mathf.Max(1f, _row.enrage_atk_rate);

        // TODO: 광폭화 공격 이펙트

        _boss.SetAttackingState(false);
        _patternRunning = false;
        _runningRoutine = null;
    }

    // 보스 또는 플레이어 주변에 겹치지 않게 더러움을 생성한다.
    // 현재 생성 개수와 반경은 CSV 컬럼이 없어서 Inspector 값 사용.
    // item_id 현재 존재하지 않아서 이후로 미룸
    private void SpawnDirtyItems()
    {
        if (_bossDirtyItemPrefab == null || ObjectPoolManager.Instance == null)
            return;

        Transform player = _boss.target;
        List<Vector3> usedPositions = new List<Vector3>();

        for (int i = 0; i < _dirtySpawnCount; i++)
        {
            bool spawnNearBoss = (i % 2 == 0);

            Vector3 center = spawnNearBoss
                ? _boss.transform.position
                : (player != null ? player.position : _boss.transform.position);

            float radius = spawnNearBoss ? _spawnRadiusFromBoss : _spawnRadiusFromPlayer;
            Vector3 spawnPos = FindNonOverlappingPosition(center, radius, usedPositions);

            PoolObject spawned = ObjectPoolManager.Instance.Spawn(_bossDirtyItemPrefab, spawnPos, Quaternion.identity);

            if (spawned != null && spawned.TryGetComponent<InGameExpItem>(out var expItem))
            {
                expItem.SetExp(0);
                expItem.SetBossSpawnedDirt(true);
            }

            usedPositions.Add(spawnPos);
        }

        // TODO:
        // item_id 기준으로 실제 아이템 리소스를 선택하도록 변경
    }

    // 이미 선택된 위치들과 일정 거리 이상 떨어진 좌표를 탐색한다.
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