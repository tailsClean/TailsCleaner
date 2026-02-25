using UnityEngine;
using UnityEngine.UI;

public class TowerUIContainer : MonoBehaviour, UIContainer
{
    [SerializeField] private Button stageButton;
    [SerializeField] private IntEventChannelSO _onStartInGame;

    void Start()
    {
        stageButton.onClick.AddListener(() => GameManager.Instance.EnterStage());
    }

}
