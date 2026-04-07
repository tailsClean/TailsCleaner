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
        if (damageable == null)
        {
            // 만약 이게 뜬다면 체력 스크립트가 PlayerZoneHandler와 같은 오브젝트에 없는 겁니다.
            Debug.LogError("PlayerZoneHandler: IDamageable 컴포넌트를 찾을 수 없습니다!");
        }
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