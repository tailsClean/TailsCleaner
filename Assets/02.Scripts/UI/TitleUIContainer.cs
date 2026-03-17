using UnityEngine;
using UnityEngine.UI;

public class TitleUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button _startButton;
    
    void Start()
    {
        _startButton.onClick.AddListener(UIManager.Instance.GoToLobby);
    }

}
