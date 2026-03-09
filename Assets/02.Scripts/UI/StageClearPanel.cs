using UnityEngine;
using UnityEngine.UI;

public class StageClearPanel : MonoBehaviour
{
    [SerializeField] Button _clearButton;
    void Awake()
    {
        gameObject.SetActive(false);
    } 
    void OnEnable()
    {
        _clearButton.onClick.AddListener(UIManager.Instance.GoToLobby);
    }
}
