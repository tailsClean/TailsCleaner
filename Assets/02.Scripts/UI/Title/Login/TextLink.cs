using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextLink : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI text;

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, null);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();

            Application.OpenURL(url);
        }
    }
}
