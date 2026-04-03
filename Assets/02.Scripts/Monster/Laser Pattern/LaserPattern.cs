using UnityEngine;
using UnityEngine.AddressableAssets;


public class LaserPattern
{
    private const string LaserPrefab = "LaserPrefab";

    private float _attackPower = 10;             // 레이저 공격 데미지
    private float _attackMultiple = 1.5f;        // 데미지 배율
    private float _laserDuration = 3.0f;         // 레이저 지속시간
    private float _laserCastTime = 2.5f;         // 레이저 시전 전 경로 표시 시간
    private float _laserSize = 1.8f;
    private int _laserCount = 6;

    private ILaserable _bossMonster;
    private Vector2 _startPos;



    public LaserPattern(ILaserable bossMonster)
    {
        _bossMonster = bossMonster;
        Init();
    }


    // 레이저 발사 시, 해당 파라미터값들을 기준으로 레이저 패턴 실행
    public void OnLaserPattern(
        float att, 
        float attMultiple,
        float laserDuration,
        float laserCastTime,
        float laserSize,
        int laserCount
        )
    {
        _attackPower = att;
        _attackMultiple = attMultiple;
        _laserDuration = laserDuration;
        _laserCastTime = laserCastTime;
        _laserSize = laserSize;
        _laserCount = laserCount;

        CalculLaserRange();
    }


    #region 내부 메서드

    // 레이저 오브젝트 생성 및 각도 계산
    private void CalculLaserRange()
    {
        _startPos = _bossMonster.MyTransform.position;

        float angle = 360 / _laserCount;
        for(int i = 0; i < _laserCount; i++)
        {
            var handle = Addressables.InstantiateAsync(LaserPrefab);
            LaserHandler laserObj = handle.Result.GetComponent<LaserHandler>();
            laserObj.transform.localScale = new Vector3(1, _laserSize * 0.5f, 0);
            laserObj.SetLaser(_attackPower, _attackMultiple, _laserDuration, _laserCastTime);
            SetLaserCast(laserObj, angle * i);
        }
    }

    // 생성된 레이저 오브젝트 세팅
    private void SetLaserCast(LaserHandler LaserObj, float rotateAngle)
    {
        // 위치 세팅
        Transform laserCastTr = LaserObj.transform;
        laserCastTr.position = _startPos;
        laserCastTr.rotation = Quaternion.Euler(0, 0, rotateAngle);
    }

    private async void Init()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(LaserPrefab);
        await handle.Task;
    }

    #endregion
}



public interface ILaserable
{
    Transform MyTransform { get; }
}