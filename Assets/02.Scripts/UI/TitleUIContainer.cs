using UnityEngine;
using UnityEngine.UI;

public class TitleUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _settingButton;
    
    void Start()
    {
        _startButton.onClick.AddListener(UIManager.Instance.GoToLobby);
         _settingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
    }

}
