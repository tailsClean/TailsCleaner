using UnityEngine;

public class SoapBubbleSkill : ActiveSkill<SoapBubbleArea, SoapBubbleModifierData>
{
    // 스킬 발동
    protected override void Active()
    {
        // 장판 플레이어 위치에 생성 (나중에 풀링)
        SoapBubbleArea area = Instantiate(_skillObjectPrefab, transform.position, Quaternion.identity);

        if (_modifierData.Tracking == true)
        {
            // 공격 방향에서 가장 가까운 적 트랜스폼 계속 추적
            //area.Init(this, _modifierData, target.transform);
        }
        else
        {
            // 그냥 정적 생성 
            area.Init(this, _modifierData);
        }
    }
}
