using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SafeZonePatternController : MonoBehaviour
{
    public static SafeZonePatternController Instance { get; private set; }

    [Header("Global Outside Effects")]
    public GameObject outsidePreviewEffect;
    public GameObject outsideActiveEffect;
    public GameObject outsideDestroyEffect;

    private readonly List<SafeZonePatternRuntime> activePatterns = new List<SafeZonePatternRuntime>();
    private int activeSafeZoneCount = 0;

    public bool HasAnyActivePattern => activePatterns.Count > 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (outsidePreviewEffect != null) outsidePreviewEffect.SetActive(false);
        if (outsideActiveEffect != null) outsideActiveEffect.SetActive(false);
    }

    public void StartPattern(float previewTime, float activeTime, float damagePerTick, float tickInterval)
    {
        StartCoroutine(PatternRoutine(previewTime, activeTime, damagePerTick, tickInterval));
    }

    private IEnumerator PatternRoutine(float previewTime, float activeTime, float damagePerTick, float tickInterval)
    {
        SafeZonePatternRuntime runtime = new SafeZonePatternRuntime(damagePerTick, tickInterval);
        activePatterns.Add(runtime);

        if (outsidePreviewEffect != null)
            outsidePreviewEffect.SetActive(true);

        yield return new WaitForSeconds(previewTime);

        if (outsidePreviewEffect != null)
            outsidePreviewEffect.SetActive(false);

        if (outsideActiveEffect != null)
            outsideActiveEffect.SetActive(true);

        runtime.isActive = true;

        float elapsed = 0f;
        while (elapsed < activeTime)
        {
            yield return new WaitForSeconds(runtime.tickInterval);
            elapsed += runtime.tickInterval;

            ApplyOutsideDamage(runtime.damagePerTick);
        }

        runtime.isFinished = true;
        activePatterns.Remove(runtime);

        if (activePatterns.Count == 0)
        {
            if (outsideActiveEffect != null)
                outsideActiveEffect.SetActive(false);

            if (outsideDestroyEffect != null)
                Instantiate(outsideDestroyEffect, transform.position, Quaternion.identity);
        }
    }

    private void ApplyOutsideDamage(float damagePerTick)
    {
        PlayerZoneHandler[] handlers = FindObjectsByType<PlayerZoneHandler>(FindObjectsSortMode.None);

        for (int i = 0; i < handlers.Length; i++)
        {
            PlayerZoneHandler handler = handlers[i];
            if (handler == null) continue;
            if (handler.IsInSafeZone) continue;

            IDamageable damageable = handler.Damageable;
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerTick);
            }
        }
    }

    public void NotifySafeZoneActivated()
    {
        activeSafeZoneCount++;
    }

    public void NotifySafeZoneDeactivated()
    {
        activeSafeZoneCount = Mathf.Max(0, activeSafeZoneCount - 1);
    }

    public bool IsAnySafeZoneCurrentlyActive()
    {
        return activeSafeZoneCount > 0;
    }

    private class SafeZonePatternRuntime
    {
        public float damagePerTick;
        public float tickInterval;
        public bool isActive;
        public bool isFinished;

        public SafeZonePatternRuntime(float damage, float interval)
        {
            damagePerTick = damage;
            tickInterval = Mathf.Max(0.05f, interval);
            isActive = false;
            isFinished = false;
        }
    }
}