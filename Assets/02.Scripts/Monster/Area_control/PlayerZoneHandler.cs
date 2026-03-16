using UnityEngine;
using System.Collections.Generic;

public class PlayerZoneHandler : MonoBehaviour
{
    [Header("Settings")]
    public float tickInterval = 0.5f; // 데미지 간격
    public float outOfSafeZoneDamage = 20f; // 안전지대 밖 기본 대미지

    private HashSet<DangerZone> currentDangerZones = new HashSet<DangerZone>();
    private int safeZoneCount = 0;
    private int activeSafeZonePatterns = 0; // 현재 활성화된 안전 지대 패턴 수
    private float timer;

    private int dangerCount = 0; // 현재 겹쳐 있는 위험 지대 개수
    public bool IsSafeZonePatternActive => activeSafeZonePatterns > 0; // 현재 안전 지대 패턴이 진행 중인지 여부
    public bool IsInSafeZone => safeZoneCount > 0;  


    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= tickInterval)
        {
            ApplyZoneDamage();
            timer = 0;
        }
    }

    private void ApplyZoneDamage()
    {
        // 보스 탐색 확인
        GameObject boss = GameObject.FindWithTag("Monster");
        if (boss == null)
        {
            // Debug.LogWarning("[확인필요] Monster 태그를 가진 보스를 찾을 수 없습니다!");
            return;
        }

        if (!boss.TryGetComponent<MonsterBase>(out var monsterBase))
        {
            Debug.LogWarning("[확인필요] 보스에게 MonsterBase 스크립트가 없습니다!");
            return;
        }

        // 현재 상태 디버그 확인
        // Debug.Log($"[상태 체크] 패턴활성:{activeSafeZonePatterns} | 안전지대내부:{safeZoneCount} | 장판수:{currentDangerZones.Count}");

        float bossPower = monsterBase.power;
        IDamageable player = GetComponent<IDamageable>();
        if (player == null) return;

        // 안전 지대 판정 로직 진입 확인
        if (IsSafeZonePatternActive)
        {
            if (!IsInSafeZone)
            {
                player.TakeDamage(bossPower);
                // Debug.Log($"<color=red>[데미지 발생]</color> 안전지대 외부! 피해량: {bossPower}");
            }
            else
            {
                Debug.Log("<color=green>[안전]</color> 플레이어가 안전지대 안에 있습니다.");
            }
        }

        // 위험 지대 판정 로직
        int actualDangerCount = currentDangerZones.Count;
        if (actualDangerCount > 0)
        {
            // 최대 2중첩까지만 적용
            int effectiveStack = Mathf.Min(actualDangerCount, 2);
            float finalDamage = bossPower * effectiveStack;

            player.TakeDamage(finalDamage);
            // Debug.Log($"<color=orange>[위험 지대 데미지]</color> 중첩수:{actualDangerCount}, 적용스택:{effectiveStack}, 총 피해:{finalDamage}");
        }
    }

    // 안전 지대 패턴 활성화/비활성화 (SafeZone 스크립트에서 호출)
    public void RegisterSafeZonePattern(bool isActive)
    {
        if (isActive) activeSafeZonePatterns++;
        else activeSafeZonePatterns = Mathf.Max(0, activeSafeZonePatterns - 1);
    }

    public void EnterDangerZone(DangerZone zone) => currentDangerZones.Add(zone);
    public void ExitDangerZone(DangerZone zone)
    {
        // zone이 null이 아니고 리스트에 있을 때만 제거
        if (zone != null && currentDangerZones.Contains(zone))
        {
            currentDangerZones.Remove(zone);
        }
    }

    public void EnterSafeZone() => safeZoneCount++;
    public void ExitSafeZone() => safeZoneCount--;
}