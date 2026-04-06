using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScrollRect : ScrollRect
{
    private bool isDraggingCustom;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        isDraggingCustom = true;
        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        isDraggingCustom = false;
        base.OnEndDrag(eventData);
    }

    protected override void LateUpdate()
    {
        if (isDraggingCustom)
        {
            // 🔥 여기서 보정 막음
            return;

        }

        base.LateUpdate();
    }
}