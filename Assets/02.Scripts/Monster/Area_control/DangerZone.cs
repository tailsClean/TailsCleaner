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
        while (isActive)
        {
            yield return new WaitForSeconds(tickInterval);

            foreach (var handler in playersInside)
            {
                if (handler == null) continue;

                IDamageable damageable = handler.Damageable;
                if (damageable != null)
                {
                    damageable.TakeDamage(damagePerTick);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null)
        {
            playersInside.Add(handler);
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