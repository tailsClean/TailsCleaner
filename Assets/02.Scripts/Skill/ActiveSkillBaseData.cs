using UnityEngine;

public class ActiveSkillBaseData : MonoBehaviour
{
    public enum ATTACK_TYPE  // 공격 방식
    {
        None = 0,
        Area = 4101,        // 장판형
        Projectile = 4102,  // 투사체형
    }

    public enum TARGETING_TYPE // 조준 방식
    {
        None = 0,
        NonTarget = 4201,   // 비대상형   (공격 방향)
        Closest = 4202,     // 조준형     (공격 방향 가장 가까운 적)
        Barrier = 4203,     // 배리어형   (플레이어 기준)
        Directional = 4204, // 이동방향형 (이동 방향)
    }

    public int MainTag;                  // 메인 태그 41001
    public string Name;                  // 스킬 이름
    public ATTACK_TYPE AttackType;       // 공격 방식 4101
    public TARGETING_TYPE TargetingType; // 조준 방식 4201


    // 생성자
    public ActiveSkillBaseData(int mainTag, string name, ATTACK_TYPE aType, TARGETING_TYPE tType)
    {
        MainTag = mainTag;
        Name = name;
        AttackType = aType;
        TargetingType = tType;
    }
}
