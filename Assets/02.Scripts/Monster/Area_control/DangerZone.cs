using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DangerZone : AreaEffector
{
    [Header("Danger")]
    public float damagePerTick = 10f;
    public float tickInterval = 0.5f;

    private readonly HashSet<PlayerZoneHandler> playersInside = new HashSet<PlayerZoneHandler>();
    private Coroutine damageRoutine;

    public void InitializeDanger(float newRadius, float newPreviewTime, float newActiveTime, float newDamagePerTick, float newTickInterval)
    {
        Initialize(newRadius, newPreviewTime, newActiveTime);
        damagePerTick = newDamagePerTick;
        tickInterval = Mathf.Max(0.05f, newTickInterval);
    }

    protected override void OnActivate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerZoneHandler handler = hit.GetComponent<PlayerZoneHandler>();
            if (handler != null)
            {
                playersInside.Add(handler);
                Debug.Log("<color=orange>[DangerZone]</color> 활성화 시점에 이미 안에 있는 플레이어 등록 완료!");
            }
        }

        if (damageRoutine != null)
            StopCoroutine(damageRoutine);

        damageRoutine = StartCoroutine(DamageTickRoutine());
    }

    protected override void OnDeactivate()
    {
        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        playersInside.Clear();
    }

    private IEnumerator DamageTickRoutine()
    {
        Debug.Log("<color=red>[DangerZone]</color> 데미지 루틴이 시작되었습니다!");
        while (isActive)
        {
            yield return new WaitForSeconds(tickInterval);

            foreach (var handler in playersInside)
            {
                if (handler == null) continue;

                // 1. 안전지대 판정 확인
                if (handler.IsInSafeZone)
                {
                    Debug.Log("<color=cyan>[Safe]</color> 플레이어가 안전지대에 있어 데미지 스킵");
                    continue;
                }

                // 2. IDamageable 확인
                IDamageable damageable = handler.Damageable;
                if (damageable != null)
                {
                    damageable.TakeDamage(damagePerTick);
                    Debug.Log($"<color=yellow>[Danger]</color> 플레이어에게 {damagePerTick} 데미지 함수 호출 성공!");
                }
                else
                {
                    Debug.LogError("<color=white>[Error]</color> 플레이어에게서 IDamageable을 찾을 수 없습니다!");
                }
            }
        }
    }

    // DangerZone.cs 수정
    private void OnTriggerEnter2D(Collider2D other)
    {
        // [중요] 이 로그가 메서드 맨 처음에 있어야 합니다!
        Debug.Log($"[DangerZone 접촉 테스트] 대상: {other.name}, Tag: {other.tag}");

        if (!other.CompareTag("Player")) return;

        // 이 아래에 isActive 체크가 있으면 예고 시간 중에는 로그가 안 뜹니다.
        if (!isActive)
        {
            Debug.Log("[DangerZone] 접촉은 했으나 아직 활성화(isActive) 전입니다.");
            return;
        }

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null)
        {
            playersInside.Add(handler);
            Debug.Log("[DangerZone] 플레이어 감지 및 리스트 추가 완료!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null)
        {
            playersInside.Remove(handler);
        }
    }
}