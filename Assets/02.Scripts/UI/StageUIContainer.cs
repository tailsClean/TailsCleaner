using UnityEngine;
using UnityEngine.UI;

public class StageUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button _settingButton;
    [SerializeField] private GameObject _exitPanel;
    public GameObject ExitPanel => _exitPanel;
    void Start()
    {
        
         _settingButton.onClick.AddListener(() => {
        Debug.Log("버튼 클릭됨!"); // 이게 뜨나요?
        UIManager.Instance.ChangeStateExitPanel();
    });
    }
}
