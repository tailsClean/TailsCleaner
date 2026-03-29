using UnityEngine;
using UnityEngine.UI;

public class HelpButtonOpener : MonoBehaviour
{
    [SerializeField] private Button _helpButton;
    [SerializeField] private HelpPopupUI _helpPopupUI;

    private void Awake()
    {
        if (_helpButton != null)
        {
            _helpButton.onClick.RemoveAllListeners();
            _helpButton.onClick.AddListener(OpenHelp);
        }
    }

    private void OpenHelp()
    {
        if (_helpPopupUI == null)
        {
            Debug.LogWarning("[HelpButtonOpener] HelpPopupUI is null.");
            return;
        }

        _helpPopupUI.Open();
    }
}