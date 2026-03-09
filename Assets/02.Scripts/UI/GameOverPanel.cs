using UnityEngine;
using UnityEngine.UI;

public class GameOverPAnel : MonoBehaviour
{
    [SerializeField] Button _retryButton;
    [SerializeField] Button _exitButton;
    void Awake()
    {
        gameObject.SetActive(false);
    } 
    void OnEnable()
    {
        _retryButton.onClick.AddListener(UIManager.Instance.GoToStage);
        _exitButton.onClick.AddListener(UIManager.Instance.GoToLobby);
    }


}
