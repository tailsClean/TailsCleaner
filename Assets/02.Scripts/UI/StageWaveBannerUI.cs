using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageWaveBannerUI : MonoBehaviour
{
    [Header("Banner UI")]
    [SerializeField] private CanvasGroup _bannerCanvasGroup;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private TextMeshProUGUI _messageText;

    [Header("Overlay UI")]
    [SerializeField] private CanvasGroup _warningOverlayCanvasGroup; // 붉은 경고 오버레이
    [SerializeField] private CanvasGroup _fadeCanvasGroup;           // 검은 암전 페이드
    [SerializeField] private Image _warningOverlayImage;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private RectTransform _fadePanelRect;

    [Header("Sprites")]
    [SerializeField] private Sprite _stageStartSprite;
    [SerializeField] private Sprite _bossStartSprite;

    [Header("Texts")]
    [TextArea]
    [SerializeField] private string _defaultStageStartMessage = "스테이지 청소 시작!";
    [TextArea]
    [SerializeField] private string _defaultBossIntroMessage = "엄청 꼬질한 녀석이 나타났어요!";

    [Header("Visual Style")]
    [SerializeField] private Color _warningOverlayColor = new Color(173f / 255f, 99f / 255f, 99f / 255f, 1f);
    [SerializeField] private float _warningOverlayVisibleAlpha = 0.55f;
    [SerializeField] private Color _fadeColor = Color.black;

    [Header("Banner Timing")]
    [SerializeField] private float _bannerFadeDuration = 0.25f;
    [SerializeField] private float _bannerHoldDuration = 1.2f;

    [Header("Boss Intro Timing")]
    [SerializeField] private float _warningTotalDuration = 2.0f;
    [SerializeField] private float _warningBlinkOnDuration = 0.3f;
    [SerializeField] private float _warningBlinkOffDuration = 0.3f;
    [SerializeField] private float _fadeOutDuration = 1.0f;
    [SerializeField] private float _blackHoldDuration = 1.0f;
    [SerializeField] private float _fadeInDuration = 1.0f;
    
    [Header("Wipe Fade")]
    [SerializeField] private bool _useSmoothStepForWipe = true;
    [SerializeField] private float _fallbackScreenWidth = 1920f;

    private void Awake()
    {
        ApplyVisualDefaults();
        ResetFadePanelWidth();
        HideAllImmediate();
    }

    private void OnValidate()
    {
        ApplyVisualDefaults();
    }

    private void ApplyVisualDefaults()
    {
        if (_warningOverlayImage != null)
        {
            _warningOverlayImage.color = _warningOverlayColor;
        }

        if (_fadeImage != null)
        {
            _fadeImage.color = _fadeColor;
        }
    }

    public IEnumerator PlayStageStart()
    {
        string message = GetStageStartMessage();
        yield return PlayBanner(_stageStartSprite, message);
    }

    public IEnumerator PlayBossIntro(Action onBlackout = null)
    {
        yield return PlayBossIntro(_defaultBossIntroMessage, onBlackout);
    }

    public IEnumerator PlayBossIntro(string message, Action onBlackout = null)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        ApplyVisualDefaults();

        SetBannerAlpha(0f);
        SetWarningOverlayAlpha(0f);
        SetFadeAlpha(1f);
        ResetFadePanelWidth();

        if (_backgroundImage != null)
            _backgroundImage.sprite = _bossStartSprite;

        if (_messageText != null)
            _messageText.text = string.IsNullOrWhiteSpace(message) ? _defaultBossIntroMessage : message;

        if (_bannerCanvasGroup != null)
            _bannerCanvasGroup.transform.SetAsFirstSibling();

        if (_warningOverlayCanvasGroup != null)
            _warningOverlayCanvasGroup.transform.SetSiblingIndex(1);

        if (_fadeCanvasGroup != null)
            _fadeCanvasGroup.transform.SetAsLastSibling();

        yield return CoBossWarningPhase();

        float fullWidth = GetFadeTargetWidth();

        // 오른쪽 -> 왼쪽으로 검은 막이 차오름
        yield return WipeFadePanel(0f, fullWidth, _fadeOutDuration);

        onBlackout?.Invoke();

        yield return new WaitForSecondsRealtime(_blackHoldDuration);

        // 다시 오른쪽 -> 왼쪽으로 지나가며 사라짐
        yield return WipeFadePanel(fullWidth, 0f, _fadeInDuration);

        HideAllImmediate();
    }

    public IEnumerator PlayBanner(Sprite sprite, string message)
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        ApplyVisualDefaults();
        ResetFadePanelWidth();
        SetFadeAlpha(1f);

        if (_backgroundImage != null)
            _backgroundImage.sprite = sprite;

        if (_messageText != null)
            _messageText.text = message;

        SetWarningOverlayAlpha(0f);

        yield return FadeCanvasGroup(_bannerCanvasGroup, 0f, 1f, _bannerFadeDuration);
        yield return new WaitForSecondsRealtime(_bannerHoldDuration);
        yield return FadeCanvasGroup(_bannerCanvasGroup, 1f, 0f, _bannerFadeDuration);

        HideAllImmediate();
    }

    private IEnumerator CoBossWarningPhase()
    {
        yield return FadeCanvasGroup(_bannerCanvasGroup, 0f, 1f, _bannerFadeDuration);

        float elapsed = 0f;
        bool overlayOn = false;

        while (elapsed < _warningTotalDuration)
        {
            overlayOn = !overlayOn;

            if (overlayOn)
            {
                SetWarningOverlayAlpha(_warningOverlayVisibleAlpha);
                yield return new WaitForSecondsRealtime(_warningBlinkOnDuration);
                elapsed += _warningBlinkOnDuration;
            }
            else
            {
                SetWarningOverlayAlpha(0f);
                yield return new WaitForSecondsRealtime(_warningBlinkOffDuration);
                elapsed += _warningBlinkOffDuration;
            }
        }

        SetWarningOverlayAlpha(0f);
        yield return FadeCanvasGroup(_bannerCanvasGroup, 1f, 0f, _bannerFadeDuration);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (cg == null)
            yield break;

        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        cg.alpha = to;
    }

    private IEnumerator WipeFadePanel(float fromWidth, float toWidth, float duration)
    {
        if (_fadePanelRect == null)
            yield break;

        if (_fadeCanvasGroup != null)
            _fadeCanvasGroup.alpha = 1f;

        if (duration <= 0f)
        {
            SetFadeWidth(toWidth);
            yield break;
        }

        float elapsed = 0f;
        SetFadeWidth(fromWidth);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (_useSmoothStepForWipe)
                t = Mathf.SmoothStep(0f, 1f, t);

            float width = Mathf.Lerp(fromWidth, toWidth, t);
            SetFadeWidth(width);

            yield return null;
        }

        SetFadeWidth(toWidth);
    }

    private void SetFadeWidth(float width)
    {
        if (_fadePanelRect == null)
            return;

        _fadePanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
    }

    private void ResetFadePanelWidth()
    {
        SetFadeWidth(0f);
    }

    private float GetFadeTargetWidth()
    {
        if (_fadePanelRect != null && _fadePanelRect.parent is RectTransform parentRect)
        {
            float width = parentRect.rect.width;
            if (width > 0f)
                return width;
        }

        return _fallbackScreenWidth;
    }

    private string GetStageStartMessage()
    {
        if (GameManager.Instance != null && GameManager.Instance._currentStage != null)
        {
            return $"{GameManager.Instance._currentStage.stage_index} 스테이지 청소 시작!";
        }

        return _defaultStageStartMessage;
    }

    private void SetBannerAlpha(float value)
    {
        if (_bannerCanvasGroup != null)
            _bannerCanvasGroup.alpha = value;
    }

    private void SetWarningOverlayAlpha(float value)
    {
        if (_warningOverlayCanvasGroup != null)
            _warningOverlayCanvasGroup.alpha = value;
    }

    private void SetFadeAlpha(float value)
    {
        if (_fadeCanvasGroup != null)
            _fadeCanvasGroup.alpha = value;
    }

    private void HideAllImmediate()
    {
        SetBannerAlpha(0f);
        SetWarningOverlayAlpha(0f);
        SetFadeAlpha(1f);
        ResetFadePanelWidth();
        gameObject.SetActive(false);
    }
}