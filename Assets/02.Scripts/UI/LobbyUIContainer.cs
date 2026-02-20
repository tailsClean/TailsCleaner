using UnityEngine;
using UnityEngine.UI;

public class LobbyUIContainer : MonoBehaviour
{
    [SerializeField] private Button TowerButton;
    void Start()
    {
        TowerButton.onClick.AddListener(() => UIManager.Instance.GoToTower());
    }

}
