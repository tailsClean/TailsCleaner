using UnityEngine;

namespace MonsterEnum
{
    public enum MONSTERMOVE
    {
        StraightChase,      // 직선 추격
        Zigzag,             // 지그재그
        Jump,               // 점프
        Flee                // 도망
    }

    public enum MONSTERTYPE
    {
        Normal,             // 일반
        Special,            // 특수
        Elite,              // 엘리트
        Boss                // 보스
    }

    public enum OVERLAPRULE
    {
        Allow,              // 중첩
        Queue,              // 대기
        Cancel,             // 취소
        Ignore              // 무시
    }

    public enum PATTERNTYPE
    {
        Projectile,         // 투사체
        AreaDenial,         // 공간제한
        CrowdControl        // 이동방해
    }

}


