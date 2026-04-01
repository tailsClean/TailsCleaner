using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ImpossibelPannel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _impossibleText;
    [SerializeField] private Button _cofirmBtn;
    
    public void SetText(string text)
    {
        _impossibleText.text = text;
    }

    public void SetListeners(UnityEngine.Events.UnityAction action)
    {
        _cofirmBtn.onClick.RemoveAllListeners();
        _cofirmBtn.onClick.AddListener(action);
    }

}
