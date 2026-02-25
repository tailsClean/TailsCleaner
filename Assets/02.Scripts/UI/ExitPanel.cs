using UnityEngine;
using UnityEngine.UI;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button exitButton;

    public void Start()
    {
        exitButton.onClick.AddListener(UIManager.Instance.GoToLobby);
        
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}

