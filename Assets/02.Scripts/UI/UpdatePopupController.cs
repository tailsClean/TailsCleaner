using UnityEngine;

public class UpdatePopupController : MonoBehaviour
{
    [SerializeField] private GameObject updatingPopup;

    // 팝업 열기
    public void ShowUpdatingPopup()
    {
        updatingPopup.SetActive(true);
    }

    // 팝업 닫기
    public void HideUpdatingPopup()
    {
        updatingPopup.SetActive(false);
    }
}
