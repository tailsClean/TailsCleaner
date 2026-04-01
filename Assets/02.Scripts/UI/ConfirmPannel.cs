using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ConfirmPannel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _confirmText;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private Button _cancelBtn;
    public void SetText(string text)
    {
        _confirmText.text = text;
    }
    public void SetListeners(UnityEngine.Events.UnityAction confirmAction, UnityEngine.Events.UnityAction cancelAction)
    {
        _confirmBtn.onClick.AddListener(confirmAction);
        _cancelBtn.onClick.AddListener(cancelAction);
    }
}



