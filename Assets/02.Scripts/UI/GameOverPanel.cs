using Unity.VisualScripting;
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
        _retryButton.onClick.AddListener(OnRetry);
        _exitButton.onClick.AddListener(OnExitGame);
        
    }

    void OnDisable()
    {
        _retryButton.onClick.RemoveListener(OnRetry);
        _exitButton.onClick.RemoveListener(OnExitGame);
    }

    private void OnExitGame()
    {
        if (StageController.Instance != null)
        {
            StageController.Instance.EndStage(StageResult.Abandon, StageFailReason.기타);
        }

        UIManager.Instance.GoToLobby();
    }

    private void OnRetry()
    {
        if(GameManager.Instance.EnergyCount <= 0)
        {
            UIManager.Instance.ImpossiblePanel.SetText("에너지가 부족합니다.");
            UIManager.Instance.ImpossiblePanel.SetListeners(() => OnExitGame());
            return;
        }
        UIManager.Instance.GoToStage();
    }
}
