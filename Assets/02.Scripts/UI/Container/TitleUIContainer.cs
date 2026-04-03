using UnityEngine;
using UnityEngine.UI;

public class TitleUIContainer : MonoBehaviour, IUIContainer
{
    [SerializeField] private GameObject _horizontalUI;
    [SerializeField] private GameObject _verticalUI;

    [SerializeField] private Button _horizontalSettingButton;
    [SerializeField] private Button _verticalSettingButton;

    void Start()
    {
        _horizontalSettingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);
        _verticalSettingButton.onClick.AddListener(UIManager.Instance.ChangeStateSettingPanel);

        OnOrientationChanged(UIManager.Instance.IsVertical);
        UIManager.Instance.OnOrientationChanged += OnOrientationChanged;

        
    

    }

    private void OnDestroy()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OnOrientationChanged -= OnOrientationChanged;
    }

    private void OnOrientationChanged(bool isVertical)
    {
        _horizontalUI.SetActive(!isVertical);
        _verticalUI.SetActive(isVertical);
    }
}
