using UnityEngine;

[CreateAssetMenu(fileName = "PassiveData", menuName = "Skill/PassiveData")]
public class PassiveSkillData : ScriptableObject
{
    [Header("기본 정보")]
    public int PassiveId;
    public string PassiveName;
    public int SubTag;

    [TextArea(2, 4)]
    public string Desc;

    [Header("패시브 모디파이어")]
    [SerializeReference] public PassiveModifier Modifier;

    // 패시브 ID
    public enum PASSIVE_ID
    {
        RaccoonCrate        = 42001,       // 매이크 라쿤 크레이트 어겐!
        CenterSwitch        = 42002,       // 목표를 중앙에 두고 스위치
        FocusAttack         = 42003,       // 집중공략
        DoubleExtraDmg      = 42004,       // 추가 추가 피해
        SuperClean          = 42005,       // SuperClean
        Bravado             = 42006,       // 객기
        VinylCoat           = 42007,       // 청소용 비닐옷
        ClassicSecret       = 42008,       // 고전비급
        BiggerBetter        = 42009,       // 더 크게! 더! 더더! 크고 아름답게!
        GoldenCrown         = 42010,       // 크고 아름다운 황금 왕관!(물에 뜹니다.)
        ADCarry             = 42011,       // 원딜의 정석
        Snowballing         = 42012,       // 스노우볼링
        Ambi                = 42013,       // 양손잡이
        Implant             = 42014,       // 기초적인 임플란트입니다
        SodaWater           = 42015,       // 탄산수
        CatLaundry          = 42016,       // 냥빨래
        NimbleBlock         = 42017,       // 하지만 이렇게 간단하게 피했습니다.
    }
}
