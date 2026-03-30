using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _settingExitBtn;
    [SerializeField] private Button _saveSetting;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    public void Start()
    {
        if (_exitButton != null) _exitButton.onClick.AddListener(OnClickExit);
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            if (_bgmSlider != null) _bgmSlider.value = SoundManager.Instance.UIBGMVolume;
            if (_sfxSlider != null) _sfxSlider.value = SoundManager.Instance.UISFXVolume;
        }

        if (_bgmSlider != null) _bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);

    }
    private void OnDisable()
    {
        if (_bgmSlider != null) _bgmSlider.onValueChanged.RemoveListener(OnBgmVolumeChanged);
        if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
    }

    private void OnBgmVolumeChanged(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetBGMVolume(value);
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(value);
    }

    private void OnClickExit()
    {
        if (StageController.Instance != null)
        {
            StageController.Instance.EndStage(StageResult.Abandon, StageFailReason.기타);
        }

        UIManager.Instance.GoToLobby();
    }
}