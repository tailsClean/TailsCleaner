using UnityEngine;
using UnityEngine.UI;

public class TitleUIContainer : MonoBehaviour, UIContainer
{

    [SerializeField] private Button _settingButton;
    
    void Start()
    {
         _settingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
    }

}
