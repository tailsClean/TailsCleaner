using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoButtonSound : MonoBehaviour, IPointerClickHandler
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    // 클릭 감지
    public void OnPointerClick(PointerEventData eventData)
    {
        // 좌클릭, 모바일 터치만
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Interactable이 켜져있을 때
        if (_button != null && _button.interactable == false)
            return;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayUISFX(UISFXName.Click);
    }
}
