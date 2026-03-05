using UnityEngine;

public class SoapBubbleSkill : ActiveSkill<SoapBubbleArea, SoapBubbleModifierData>
{
    // 스킬 발동

    protected override void OnActive(int index, int totalCount)
    {
        // 장판 플레이어 위치에 생성
        //SoapBubbleArea area = Instantiate(_skillObjectPrefab, transform.position, Quaternion.identity);
        SoapBubbleArea area = SpawnFromPool<SoapBubbleArea>(_skillObjectPrefab, transform.position, Quaternion.identity);

        // 초기화
        if(area != null) area.Init(this, _modifierData);
    }
}
