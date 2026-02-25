using UnityEngine;
using UnityEngine.UI;

public class LobbyUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button _towerButton;
    
    void Start()
    {
        _towerButton.onClick.AddListener(() => UIManager.Instance.GoToTower());
    }

}
