using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestingCode : MonoBehaviour, IBeginDragHandler
{
    [ContextMenu("가자")]
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("자식 드래그 시작");
    }
}
