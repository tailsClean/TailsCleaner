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
        // 이미 동일한 루틴이 돌고 있다면 새로 시작하지 않음 (중첩 방지)
        if (HasAnyActivePattern)
        {
            Debug.Log("<color=white>[Controller]</color> 이미 패턴이 실행 중이므로 중복 실행을 무시합니다.");
            return;
        }

        Debug.Log($"<color=orange>[Controller]</color> 패턴 시작! 예고:{previewTime}s, 유지:{activeTime}s");
        StartCoroutine(PatternRoutine(previewTime, activeTime, damagePerTick, tickInterval));
    }

    private IEnumerator PatternRoutine(float previewTime, float activeTime, float damagePerTick, float tickInterval)
    {
        SafeZonePatternRuntime runtime = new SafeZonePatternRuntime(damagePerTick, tickInterval);
        activePatterns.Add(runtime);

        if (outsidePreviewEffect != null) outsidePreviewEffect.SetActive(true);

        yield return new WaitForSeconds(previewTime);

        if (outsidePreviewEffect != null) outsidePreviewEffect.SetActive(false);
        if (outsideActiveEffect != null) outsideActiveEffect.SetActive(true);

        runtime.isActive = true;
        float elapsed = 0f;

        // [로그 2] 루프 진입 직전 상태 확인
        Debug.Log($"<color=yellow>[Controller]</color> 데미지 루프 진입! 목표 유지시간: {activeTime}");

        if (activeTime <= 0)
        {
            Debug.LogWarning("<color=red>[Warning]</color> 유지시간(duration)이 0이라서 데미지 루프가 즉시 종료됩니다!");
        }

        while (elapsed < activeTime)
        {
            yield return new WaitForSeconds(runtime.tickInterval);
            elapsed += runtime.tickInterval;

            // [로그 3] 실제 데미지 함수 호출 여부
            Debug.Log($"<color=red>[Controller]</color> {elapsed:F1}초 경과... 데미지 시도 중!");
            ApplyOutsideDamage(runtime.damagePerTick);
        }

        Debug.Log("<color=white>[Controller]</color> 패턴 종료");
        // ... 이하 기존 코드 동일
    }

    private void ApplyOutsideDamage(float damagePerTick)
    {
        PlayerZoneHandler[] handlers = FindObjectsByType<PlayerZoneHandler>(FindObjectsSortMode.None);

        // 1. 플레이어를 찾았는지 확인
        if (handlers.Length == 0)
        {
            Debug.LogWarning("[패턴컨트롤러] 씬에서 PlayerZoneHandler를 찾을 수 없습니다!");
            return;
        }

        for (int i = 0; i < handlers.Length; i++)
        {
            PlayerZoneHandler handler = handlers[i];
            if (handler == null) continue;

            // 2. 현재 플레이어의 안전 상태 확인
            if (handler.IsInSafeZone)
            {
                // 안전지대 안이라서 데미지를 안 주는 상태
                Debug.Log($"[안전] {handler.name}님은 보호받고 있습니다.");
                continue;
            }

            // 3. 데미지 전달 시도
            IDamageable damageable = handler.Damageable;
            if (damageable != null)
            {
                damageable.TakeDamage(damagePerTick);
                Debug.Log($"<color=red>[필드 데미지]</color> {handler.name}에게 {damagePerTick} 데미지 부여!");
            }
            else
            {
                Debug.LogError($"[오류] {handler.name}에게 IDamageable이 없습니다!");
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