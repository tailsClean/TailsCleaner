using UnityEngine;
using DG.Tweening;
using System.Collections;

public class LaserHandler : MonoBehaviour
{
    [SerializeField] private LaserBase _castLaser;
    [SerializeField] private LaserBase _laser;

    [Header("깜빡임 효과 값 세팅(공격 기능은 사용X)")]
    [SerializeField] private float _blinkColorAlpha = 0.2f;
    [SerializeField] private float _blinkCycle = 0.5f;

    [Header("<color=green>================== 보기용 필드(수정해도 의미 없음) ==============================")]
    [Header("레이저 발사 데이터")]
    [SerializeField] private float _damage;             // 레이저 데미지
    [SerializeField] private float _laserDuration;      // 레이저 지속시간
    [SerializeField] private float _laserCastTime;      // 레이저 시전 전 경로 표시 시간
    
    private SpriteRenderer _laserSprite;                // 깜빡임 효과를 위한 레이저 스프라이트렌더러



    private void Awake()
    {
        if (_castLaser == null || _laser == null)
        { Debug.LogError("레이저 오브젝트가 프리펩에서 연결되지 않았습니다.", this); return; }

        if (!_laser.TryGetComponent<Collider2D>(out var collider))
            Debug.LogWarning("레이저에 콜라이더가 없습니다.", this);

        _laserSprite = _castLaser.GetComponent<SpriteRenderer>();

        ChangeLaser(isCasting: true);
    }

    private void OnEnable()
    {
        StartCoroutine(StartLaser());
    }

    private void OnDisable()
    {
        ChangeLaser(isCasting: true);
    }



    // 레이저 발사용 값 추가
    public void SetLaser(float att, float duration, float castingTime)
    {
        _damage = att;
        _laserDuration = duration;
        _laserCastTime = castingTime;

        _laser.Init(_damage);
    }



    #region 내부 메서드

    // 입력된 시간에 맞춰서 레이저 발사 메서드
    private IEnumerator StartLaser()
    {
        yield return null;

        PlayBlink();
        yield return new WaitForSeconds(_laserCastTime);

        if (_laserSprite != null)
            _laserSprite.DOKill();

        // 캐스팅 종료 후 레이저 발사
        ChangeLaser(isCasting: false);
        yield return new WaitForSeconds(_laserDuration);
        Destroy(gameObject);
    }

    // 깜빡임 효과
    private void PlayBlink()
    {
        if (_laserSprite == null)
            return;

        _laserSprite.DOFade(_blinkColorAlpha, _blinkCycle)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    // 캐스팅중인지에 따라 레이저 활성화를 변경
    private void ChangeLaser(bool isCasting)
    {
        if(isCasting)
        {
            _castLaser.gameObject.SetActive(true);
            _laser.gameObject.SetActive(false);
        }

        else
        {
            _castLaser.gameObject.SetActive(false);
            _laser.gameObject.SetActive(true);
        }
    }

    #endregion
}
