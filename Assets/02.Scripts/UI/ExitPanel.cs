using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Slider _bGMSlider;
    [SerializeField] private Slider _skillSFXSlider;
    [SerializeField] private Slider _monsterSFXSlider;

    public void Start()
    {
        _exitButton.onClick.AddListener(OnClickExit);
        _bGMSlider?.onValueChanged.AddListener (value => SoundManager.Instance.SetBGMVolume(value));
        _skillSFXSlider?.onValueChanged.AddListener(value => SoundManager.Instance.SetSkillSFXVolume(value));
        _monsterSFXSlider?.onValueChanged.AddListener(value => SoundManager.Instance.SetMonsterSFXVolume(value));

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