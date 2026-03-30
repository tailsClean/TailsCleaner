# if UNITY_EDITOR
using System.Collections;
using UnityEngine;


public class TsetLaserMonser : MonoBehaviour, ILaserable
{
    public LaserPattern laserPattern;

    [Header("")]
    public float _attackPower = 10;             // 레이저 공격 데미지
    public float _laserDuration = 3.0f;         // 레이저 지속시간
    public float _laserCastTime = 2.5f;         // 레이저 시전 전 경로 표시 시간
    public float _laserSize = 1.8f;
    public int _laserCount = 6;


    public Transform MyTransform => transform;

    private void Awake()
    {
        laserPattern = new LaserPattern(this);
    }


    [ContextMenu("레이저 발사")]
    public void Shot()
    {
        laserPattern.OnLaserPattern(
            _attackPower,
            _laserDuration,
            _laserCastTime,
            _laserSize,
            _laserCount
            );
    }
}
#endif