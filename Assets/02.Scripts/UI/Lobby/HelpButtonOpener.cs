using UnityEngine;
using UnityEngine.UI;

public class HelpButtonOpener : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private HelpPopupUI _popup;

    private void Awake()
    {
        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(TogglePopup);
        }
    }

    private void TogglePopup()
    {
        if (_popup != null)
            _popup.Toggle();
        else
            Debug.LogWarning("[HelpButtonOpener] HelpPopupUI 연결 안됨");
    }
}