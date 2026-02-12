using UnityEngine;

public class BubbleSkill : ActiveSkill
{
    // 스킬 발동
    protected override void Active()
    {
        Debug.Log("비누 거품 실행");
    }
}
