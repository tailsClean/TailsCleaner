using UnityEngine;

using UnityEngine.UI;

public sealed class StageTimerTextUI : MonoBehaviour
{
    [Header("UI (Text)")]
    [SerializeField] private Text _mainTimerText;   // 00:00 (경과)
    [SerializeField] private Text _bossTimerText;   // 00:00 (남은시간)

    [Header("Binding")]
    [SerializeField] private StageController _stageController;

    [Header("Options")]
    [SerializeField] private bool _showBossTimerOnlyWhenActive = true;
    [SerializeField] private string _mainPrefix = "Main";
    [SerializeField] private string _bossPrefix = "Boss";

    private StageEvents _events;
    private bool _bossSeen;

    private void Awake()
    {
        if (_bossTimerText != null)
            _bossTimerText.gameObject.SetActive(!_showBossTimerOnlyWhenActive);
    }

    private void OnEnable()
    {
        TryBind();
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void TryBind()
    {
        if (_events != null) return;

        if (_stageController == null)
        {
            Debug.LogWarning("[StageTimerTextUI] StageController is not assigned.");
            return;
        }

        Debug.LogWarning("[StageTimerTextUI] Call Bind(StageEvents) from StageController after StartStage.");
    }

    public void Bind(StageEvents events)
    {
        Unbind();

        _events = events;
        if (_events == null) return;

        _events.OnMainSecondTick += HandleMainSecondTick;
        _events.OnBossSecondTick += HandleBossSecondTick;
        _events.OnStageResult += HandleStageResult;

        SetMainText(0);
        SetBossText(0);
    }

    private void Unbind()
    {
        if (_events == null) return;

        _events.OnMainSecondTick -= HandleMainSecondTick;
        _events.OnBossSecondTick -= HandleBossSecondTick;
        _events.OnStageResult -= HandleStageResult;
        _events = null;
    }

    private void HandleMainSecondTick(int seconds)
    {
        SetMainText(seconds);
    }

    private void HandleBossSecondTick(int secondsLeft)
    {
        _bossSeen = true;

        if (_bossTimerText != null && _showBossTimerOnlyWhenActive)
            _bossTimerText.gameObject.SetActive(true);

        SetBossText(secondsLeft);
    }

    private void HandleStageResult(StageResult result, StageFailReason reason)
    {
        if (_bossTimerText != null && _showBossTimerOnlyWhenActive)
            _bossTimerText.gameObject.SetActive(false);
    }

    private void SetMainText(int seconds)
    {
        if (_mainTimerText == null) return;
        _mainTimerText.text = $"{_mainPrefix} {FormatMMSS(seconds)}";
    }

    private void SetBossText(int seconds)
    {
        if (_bossTimerText == null) return;

        if (_showBossTimerOnlyWhenActive && !_bossSeen)
        {
            _bossTimerText.gameObject.SetActive(false);
            return;
        }

        _bossTimerText.text = $"{_bossPrefix} {FormatMMSS(seconds)}";
    }

    private static string FormatMMSS(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        int mm = totalSeconds / 60;
        int ss = totalSeconds % 60;
        return mm.ToString("00") + ":" + ss.ToString("00");
    }
}