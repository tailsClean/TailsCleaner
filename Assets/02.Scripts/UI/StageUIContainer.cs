using UnityEngine;
using UnityEngine.UI;

public class StageUIContainer : MonoBehaviour, UIContainer // stageUI에서 연결되어야 할 UI 요소들을 참조하는 클래스
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
