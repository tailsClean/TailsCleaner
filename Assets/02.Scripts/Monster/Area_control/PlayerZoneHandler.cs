using UnityEngine;
using System.Collections.Generic;

public class PlayerZoneHandler : MonoBehaviour
{
    private readonly HashSet<SafeZone> currentSafeZones = new HashSet<SafeZone>();
    private IDamageable damageable;

    public bool IsInSafeZone => currentSafeZones.Count > 0;
    public IDamageable Damageable => damageable;

    private void Awake()
    {
        damageable = GetComponent<IDamageable>();
    }

    public void EnterSafeZone(SafeZone zone)
    {
        if (zone != null)
            currentSafeZones.Add(zone);
    }

    public void ExitSafeZone(SafeZone zone)
    {
        if (zone != null)
            currentSafeZones.Remove(zone);
    }

    private void OnDisable()
    {
        currentSafeZones.Clear();
    }
}