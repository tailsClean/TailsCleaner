using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Button _settingExitBtn;
    [SerializeField] private Button _saveSetting;
    [SerializeField] private Slider _bGMSlider;
    [SerializeField] private Slider _skillSFXSlider;
   

    public void Start()
    {
        _exitButton.onClick.AddListener(UIManager.Instance.GoToLobby);
        _settingExitBtn.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        _bGMSlider?.onValueChanged.AddListener (value => SoundManager.Instance.SetBGMVolume(value));
        _skillSFXSlider?.onValueChanged.AddListener(value => SoundManager.Instance.SetSkillSFXVolume(value));

    }
    
}

