using UnityEngine;
using UnityEngine.UI;

public class TowerUIContainer : MonoBehaviour
{
    [SerializeField] private Button stageButton;
    void Start()
    {
        stageButton.onClick.AddListener(() => UIManager.Instance.GoToStage());
    }

    
}
