using UnityEngine;

public class UIScaleChanger : MonoBehaviour
{
    [Header("사이즈 조절")]
    [SerializeField] private float _poneScale = 1.79f;
    [SerializeField] private float _padScale = 1.35f;

    private RectTransform _myRect;
    private int _screenWidth;
    private int _screenHeight;


    private void Awake()
    {
        _myRect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        ScaleChange();
        UIManager.Instance.OnOrientationChanged += _ =>  ScaleChange();
    }
    private void OnDestroy()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnOrientationChanged -= _ => ScaleChange();
        }
    }

    public void ScaleChange()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;

        float ratio = (float)_screenWidth / _screenHeight;

        if (ratio < 0.6f)
        {
            // 세로 긴 폰 (예: 9:19)
            _myRect.localScale = new Vector2(_poneScale, _poneScale);
        }
        else
        {
            // 태블릿 or PC
            _myRect.localScale = new Vector2(_padScale, _padScale);
        }
    }
}
