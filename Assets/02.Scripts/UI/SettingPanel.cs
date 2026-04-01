using UnityEngine;
using UnityEngine.SceneManagement;
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

    private void Start()
    {
        if (_dungeonExitButton != null) _dungeonExitButton.onClick.AddListener(OnClickExit);
        if (_settingExitBtn != null) _settingExitBtn.onClick.AddListener(OnClickExitSetting);
        if (_saveSetting != null) _saveSetting.onClick.AddListener(OnClickSaveSetting); 
        UpdateButton();

        _horizontalBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.SetOrientation(false); // 가로
            UpdateButton();
        });

        _verticalBtn.onClick.AddListener(() =>
        {
            UIManager.Instance.SetOrientation(true); // 세로
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

        LoadSettings();
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
        if(SceneManager.GetActiveScene().name != "StageScene")
        {
            UIManager.Instance.ChangeStateImpossiblePanel();
            UIManager.Instance.ImpossiblePanel.SetText("나갈 수 있는 공간이 아니에요!");
            UIManager.Instance.ImpossiblePanel.SetListeners(() => UIManager.Instance.ChangeStateImpossiblePanel());
            
            return;
        }
        else
        {
            UIManager.Instance.ChangeStateConfirmPanel();
            UIManager.Instance.ConfirmPanel.SetText("던전을 나가시겠습니까?");
            UIManager.Instance.ConfirmPanel.SetListeners(() =>
            {
                 if (StageController.Instance != null)
                {
                    StageController.Instance.EndStage(StageResult.Abandon, StageFailReason.기타);
                }

                UIManager.Instance.GoToLobby();
                UIManager.Instance.ChangeStateConfirmPanel();
            }, () => UIManager.Instance.ChangeStateConfirmPanel());

           
        } 
    }
    private void UpdateButton()
    {
        bool isVertical = UIManager.Instance.IsVertical;
        _horizontalBtn.interactable = isVertical;   
        _verticalBtn.interactable = !isVertical; 
    }
    private void OnClickExitSetting()
    {
       gameObject.SetActive(false);
    }
    private void OnClickSaveSetting()
    {
        PlayerPrefs.SetFloat("BGMVolume", _bgmSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", _sfxSlider.value);
        PlayerPrefs.SetInt("IsVertical", UIManager.Instance.IsVertical ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            float bgmVolume = PlayerPrefs.GetFloat("BGMVolume");
            if (SoundManager.Instance != null) SoundManager.Instance.SetBGMVolume(bgmVolume);
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(sfxVolume);
        }

        if (PlayerPrefs.HasKey("IsVertical"))
        {
            bool isVertical = PlayerPrefs.GetInt("IsVertical") == 1;
            UIManager.Instance.SetOrientation(isVertical);
            UpdateButton();
        }
    }
}