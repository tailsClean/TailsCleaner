using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ZoneTestCaller : MonoBehaviour
{
    [System.Serializable]
    public class ZonePattern
    {
        public string patternName = "New Pattern";
        public bool onlyUseThis; // 이 패턴만 반복 실행하고 싶을 때 체크
        public GameObject zonePrefab;
        public bool isSafeZone;

        [Header("Spawn Settings")]
        public SpawnTarget targetType;
        public int count = 1;
        public float range = 5f;
        public float radius = 3f;

        [Header("Time Settings")]
        public float startDelay = 0.5f;
        public float cooldown = 3.0f;

        [HideInInspector] public bool isCooldown = false;
        public bool triggerPattern;
    }

    public enum SpawnTarget { Player, Monster }

    [Header("References")]
    public Transform playerTransform;
    public Transform monsterTransform;

    [Header("Auto Play Settings")]
    public bool autoPlay = false;
    public bool useRandomOrder = false;
    public float timeBetweenPatterns = 2.0f;

    [Header("Patterns List")]
    public List<ZonePattern> patterns = new List<ZonePattern>();

    private ZoneSpawner spawner;
    private Coroutine autoPlayCoroutine;

    void Start()
    {
        spawner = GetComponent<ZoneSpawner>();
    }

    void Update()
    {
        // 수동 실행
        foreach (var pattern in patterns)
        {
            if (pattern.triggerPattern)
            {
                pattern.triggerPattern = false;
                if (!pattern.isCooldown) StartCoroutine(ExecutePatternRoutine(pattern));
            }
        }

        // 2. 자동 실행 스위치 감지
        if (autoPlay && autoPlayCoroutine == null)
        {
            autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
        }
        else if (!autoPlay && autoPlayCoroutine != null)
        {
            StopCoroutine(autoPlayCoroutine);
            autoPlayCoroutine = null;
        }
    }

    private IEnumerator AutoPlayRoutine()
    {
        int currentIndex = 0;

        while (autoPlay)
        {
            if (patterns.Count == 0) yield break;

            ZonePattern p = null;

            // Only Use This가 체크된 패턴이 있는지 리스트에서 찾음
            ZonePattern forcedPattern = patterns.Find(x => x.onlyUseThis);

            if (forcedPattern != null)
            {
                // 체크된 게 있다면 그것만 실행
                p = forcedPattern;
            }
            else
            {
                // 체크된 게 없다면 기존 방식대로 (랜덤 혹은 순서대로) 선택
                int indexToRun = useRandomOrder ? Random.Range(0, patterns.Count) : currentIndex;
                p = patterns[indexToRun];
                currentIndex = (currentIndex + 1) % patterns.Count;
            }
            

            yield return StartCoroutine(ExecutePatternRoutine(p));
            yield return new WaitForSeconds(timeBetweenPatterns);
        }
    }

    private IEnumerator ExecutePatternRoutine(ZonePattern p)
    {
        p.isCooldown = true;
        if (p.startDelay > 0) yield return new WaitForSeconds(p.startDelay);

        ExecutePattern(p);

        yield return new WaitForSeconds(p.cooldown);
        p.isCooldown = false;
    }

    private void ExecutePattern(ZonePattern p)
    {
        if (spawner == null) return;
        Transform target = (p.targetType == SpawnTarget.Player) ? playerTransform : monsterTransform;
        if (target != null)
        {
            spawner.SpawnZone(p.zonePrefab, target, p.count, p.range, p.isSafeZone, p.radius);
        }
    }
}