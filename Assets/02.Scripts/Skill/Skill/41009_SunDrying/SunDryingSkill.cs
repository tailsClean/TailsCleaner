using UnityEngine;

public class SunDryingSkill : ActiveSkill<SunDryingArea, SunDryingModifierData>
{ 
    // 현재 활성화된 일광건조 수 (0 이면 꺼진 상태)
    private int _activeAreaCount = 0;

    public bool IsAreaActive => _activeAreaCount > 0;


    protected override void OnActive(int index, int totalCount)
    {
        // 생성
        SunDryingArea area = Instantiate(_skillObjectPrefab, transform.position, Quaternion.identity);

        // 초기화
        area.Init(this, _modifierData);

        // 활성화 일광건조 수 증가
        _activeAreaCount++;

        // 활성화 모디파이어 실행
        OnActivate();
    }
    
    
    // SunDryingArea.OnExpire에서 호출
    // 일광건조 만료 시
    public void OnAreaExpired()
    {
        // 활성화 일광건조 감소
        _activeAreaCount--;

        // 마지막 일광건조가 꺼지면
        // 비활성화 모디파이어 실행
        if (_activeAreaCount == 0)
            OnDeactivate();
    }
    
    
    // 켜질 때
    private void OnActivate()
    {
        // 기상!
        // 시전 시 체력 5% 회복
        if (_modifierData.HealOnActivate)
        {
            Debug.Log("[SunDrying] 기상! - 체력 회복");
        }

        // 이불 털기
        // 켜질 때 범위 내 적 넉백
        if (_modifierData.KnockbackOnActivate)
        {
            Debug.Log("[SunDrying] 이불 털기 - 넉백");
        }

        // 이불 두르기
        // 켜지면 방어력 버프 해제
        if (_modifierData.DefenseOnInactive)
        {
            Debug.Log("[SunDrying] 이불 두르기 - 방어력 버프 해제");
        }

        // 으슬으슬
        // 맵 전체 적 슬로우 적용, 스폰되는 적도 포함
        if (_modifierData.SlowOnArea)
        {
            Debug.Log("[SunDrying] 으슬으슬 - 전체 적 슬로우 적용");
        }
    }

    // 꺼질 때
    private void OnDeactivate()
    {
        // 두꺼운 이불
        // 꺼질 때 방어막 생성
        if (_modifierData.ShieldOnDeactivate)
        {
            Debug.Log("[SunDrying] 두꺼운 이불 - 방어막 생성");
        }

        // 이불 두르기
        // 꺼진 동안 방어력 증가
        if (_modifierData.DefenseOnInactive)
        {
            Debug.Log("[SunDrying] 이불 두르기 - 방어력 증가");
        }

        // 으슬으슬
        // 맵 전체 적 슬로우 해제
        if (_modifierData.SlowOnArea)
        {
            Debug.Log("[SunDrying] 으슬으슬 - 전체 적 슬로우 해제");
        }
    }
}
