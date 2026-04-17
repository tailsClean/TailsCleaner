using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZoneTestCaller : MonoBehaviour
{
    public enum SpawnTarget
    {
        Player,
        Monster
    }

    public enum ZonePatternType
    {
        DangerZone,
        SafeZone
    }

    [System.Serializable]
    public class ZonePattern
    {
        public string patternName = "New Pattern";
        public bool onlyUseThis;
        public bool triggerPattern;

        [Header("Pattern Type")]
        public ZonePatternType patternType;

        [Header("Prefab")]
        public GameObject zonePrefab;

        [Header("Spawn")]
        public SpawnTarget targetType;
        public int count = 1;
        public float range = 5f;
        public float radius = 3f;

        [Header("Timing")]
        public float startDelay = 0.5f;
        public float previewTime = 2f;
        public float activeTime = 5f;
        public float cooldown = 3f;

        [Header("Damage")]
        public float damagePerTick = 10f;
        public float damageInterval = 0.5f;

        [HideInInspector] public bool isCooldown = false;
    }

    [Header("References")]
    public Transform playerTransform;
    public Transform monsterTransform;
    public ZoneSpawner spawner;

    [Header("Auto Play")]
    public bool autoPlay = false;
    public bool useRandomOrder = false;
    public bool runImmediatelyAfterCooldown = true;
    public float timeBetweenPatterns = 2f;

    [Header("Patterns")]
    public List<ZonePattern> patterns = new List<ZonePattern>();

    private Coroutine autoPlayCoroutine;

    private void Awake()
    {
        if (spawner == null)
            spawner = GetComponent<ZoneSpawner>();
    }

    private void Update()
    {
        for (int i = 0; i < patterns.Count; i++)
        {
            if (patterns[i].triggerPattern)
            {
                patterns[i].triggerPattern = false;

                if (!patterns[i].isCooldown)
                    StartCoroutine(ExecutePatternRoutine(patterns[i]));
            }
        }

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
            if (patterns.Count == 0)
                yield break;

            ZonePattern forcedPattern = patterns.Find(x => x.onlyUseThis);
            ZonePattern patternToRun = forcedPattern;

            if (patternToRun == null)
            {
                int index = useRandomOrder ? Random.Range(0, patterns.Count) : currentIndex;
                patternToRun = patterns[index];
                currentIndex = (currentIndex + 1) % patterns.Count;
            }

            if (patternToRun != null && !patternToRun.isCooldown)
            {
                yield return StartCoroutine(ExecutePatternRoutine(patternToRun));
            }
            else
            {
                yield return null;
            }

            if (!runImmediatelyAfterCooldown && timeBetweenPatterns > 0f)
            {
                yield return new WaitForSeconds(timeBetweenPatterns);
            }
        }
    }

    private IEnumerator ExecutePatternRoutine(ZonePattern pattern)
    {
        pattern.isCooldown = true;

        if (pattern.startDelay > 0f)
            yield return new WaitForSeconds(pattern.startDelay);

        ExecutePattern(pattern);

        float lifetime = pattern.previewTime + pattern.activeTime;
        if (lifetime > 0f)
            yield return new WaitForSeconds(lifetime);

        if (pattern.cooldown > 0f)
            yield return new WaitForSeconds(pattern.cooldown);

        pattern.isCooldown = false;
    }

    private void ExecutePattern(ZonePattern pattern)
    {
        if (spawner == null)
            return;

        Transform target = GetTargetTransform(pattern.targetType);
        if (target == null)
            return;

        switch (pattern.patternType)
        {
            case ZonePatternType.DangerZone:
                spawner.SpawnDangerZones(
                    pattern.zonePrefab,
                    target,
                    pattern.count,
                    pattern.range,
                    pattern.radius,
                    pattern.previewTime,
                    pattern.activeTime,
                    pattern.damagePerTick,
                    pattern.damageInterval
                );
                break;

            case ZonePatternType.SafeZone:
                SafeZonePatternController.Instance?.StartPattern(
                    pattern.previewTime,
                    pattern.activeTime,
                    pattern.damagePerTick,
                    pattern.damageInterval
                );

                spawner.SpawnSafeZones(
                    pattern.zonePrefab,
                    target,
                    pattern.count,
                    pattern.range,
                    pattern.radius,
                    pattern.previewTime,
                    pattern.activeTime
                );
                break;
        }
    }

    private Transform GetTargetTransform(SpawnTarget targetType)
    {
        return targetType == SpawnTarget.Player ? playerTransform : monsterTransform;
    }
}