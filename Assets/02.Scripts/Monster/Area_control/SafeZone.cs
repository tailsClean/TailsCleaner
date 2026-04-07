using UnityEngine;
using System.Collections.Generic;

public class SafeZone : AreaEffector
{
    private Pattern patternData;
    private readonly HashSet<PlayerZoneHandler> playersInside = new HashSet<PlayerZoneHandler>();

    public void InitializeSafe(Pattern data)
    {
        if (data == null) return;
        this.patternData = data;

        if (SafeZonePatternController.Instance != null && !SafeZonePatternController.Instance.HasAnyActivePattern)
        {
            float finalDamage = data.damage_multiply;
            float interval = data.area_damage_interval > 0 ? data.area_damage_interval : 0.5f;

            SafeZonePatternController.Instance.StartPattern(data.cast_time, data.duration, finalDamage, interval);
            Debug.Log($"<color=cyan>[Pattern]</color> 전역 데미지 시작 명령 발송");
        }

        // 위에서 만든 숫자형 초기화 함수를 호출하여 중복 코드 방지
        InitializeSafe(data.area_radius, data.cast_time, data.duration);
    }

    public void InitializeSafe(float newRadius, float newPreviewTime, float newActiveTime)
    {
        this.radius = newRadius;

        
        if (SafeZonePatternController.Instance != null)
        {
            SafeZonePatternController.Instance.StartPattern(newPreviewTime, newActiveTime, 10f, 0.5f);
        }

        Initialize(this.radius, newPreviewTime, newActiveTime);
        UpdateVisuals(newRadius);
    }

    private void UpdateVisuals(float r)
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.color = new Color(0, 1, 0, 0.2f);
        transform.localScale = new Vector3(r * 2, r * 2, 1);
    }

    protected override void OnActivate()
    {
        // 장판이 켜질 때 컨트롤러에 "나 켜졌어"라고 알림
        SafeZonePatternController.Instance?.NotifySafeZoneActivated();

        // 주변 플레이어를 찾아서 "너 지금 안전해"라고 등록
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;
            PlayerZoneHandler handler = hit.GetComponent<PlayerZoneHandler>();

            if (handler != null && playersInside.Add(handler))
            {
                handler.EnterSafeZone(this); // HashSet에 추가됨 (Count 증가)
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
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler == null) return;

        if (playersInside.Add(handler))
        {
            if (isActive)
            {
                handler.EnterSafeZone(this);
                Debug.Log("안전 지대 활성화 및 진입!");
            }
            else
            {
                Debug.Log("안전 지대 예고 범위 진입 (대기 중...)");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerZoneHandler handler = other.GetComponent<PlayerZoneHandler>();
        if (handler != null && playersInside.Remove(handler))
        {
            handler.ExitSafeZone(this);
            Debug.Log("안전 지대 탈출!");
        }
    }
}