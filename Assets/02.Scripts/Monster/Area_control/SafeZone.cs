using UnityEngine;
using System.Collections.Generic;

public class SafeZone : AreaEffector
{
    private readonly HashSet<PlayerZoneHandler> playersInside = new HashSet<PlayerZoneHandler>();

    public void InitializeSafe(float newRadius, float newPreviewTime, float newActiveTime)
    {
        Initialize(newRadius, newPreviewTime, newActiveTime);
    }

    protected override void OnActivate()
    {
        SafeZonePatternController.Instance?.NotifySafeZoneActivated();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerZoneHandler handler = hit.GetComponent<PlayerZoneHandler>();
            if (handler != null && playersInside.Add(handler))
            {
                handler.EnterSafeZone(this);
            }
        }
    }

    protected override void OnDeactivate()
    {
        foreach (var handler in playersInside)
        {
            if (handler != null)
                handler.ExitSafeZone(this);
        }

        playersInside.Clear();

        SafeZonePatternController.Instance?.NotifySafeZoneDeactivated();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null && playersInside.Add(handler))
        {
            handler.EnterSafeZone(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null && playersInside.Remove(handler))
        {
            handler.ExitSafeZone(this);
        }
    }
}