using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button _dungeonExitButton;
    [SerializeField] private Button _settingExitBtn;
    [SerializeField] private Button _saveSetting;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Button _horizontalBtn;
    [SerializeField] private Button _verticalBtn;

    private float _prevBgmVolume;
    private float _prevSfxVolume;

    public void Start()
    {
        if (_dungeonExitButton != null) _dungeonExitButton.onClick.AddListener(OnClickExit);
        if (_settingExitBtn != null) _settingExitBtn.onClick.AddListener(OnClickExitSetting);
        UpdateButton();

        _horizontalBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.IsVertical = false; // 가로
            UpdateButton();
        });

        _verticalBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.IsVertical = true; // 세로
            UpdateButton();
        });

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
    private void UpdateButton()
    {
        bool isVertical = UIManager.Instance.IsVertical;
        _horizontalBtn.interactable = isVertical;   // 세로일 때 가로버튼 활성
        _verticalBtn.interactable = !isVertical; 
    }
    private void OnClickExitSetting()
    {
       gameObject.SetActive(false);
    }
}