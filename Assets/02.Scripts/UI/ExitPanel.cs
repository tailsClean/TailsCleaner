using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class ExitPanel : MonoBehaviour
{
    [SerializeField] private Button _exitButton;
    [SerializeField] private Slider _bGMSlider;

    public void Start()
    {
        _exitButton.onClick.AddListener(UIManager.Instance.GoToLobby);
        
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        _bGMSlider?.onValueChanged.AddListener (value => SoundManager.Instance.SetBGMVolume(value));

    }
}

