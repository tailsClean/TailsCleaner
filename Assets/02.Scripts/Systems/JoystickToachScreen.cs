using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoystickToachScreen : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private GameObject _joystick;
    [SerializeField] private GameObject _handle;
    [SerializeField] private List<Image> _joystickImage;
    

    private void Awake()
    {
        JoystickActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _joystick.transform.position = eventData.position;
        _joystick?.SetActive(true);
        JoystickActive(true);

        // 강제로 Stick에 PointerDown 전달
        ExecuteEvents.Execute(
            _handle,
            eventData,
            ExecuteEvents.pointerDownHandler
        );
    }

    public void OnDrag(PointerEventData eventData)
    {
        ExecuteEvents.Execute(
            _handle,
            eventData,
            ExecuteEvents.dragHandler
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ExecuteEvents.Execute(
            _handle,
            eventData,
            ExecuteEvents.pointerUpHandler
        );

        JoystickActive(false);
    }

    private void JoystickActive(bool active)
    {
        foreach(var joystick in _joystickImage)
        {
            var color = joystick.color;

            if (active)
                color.a = 1;
            else
                color.a = 0;

            joystick.color = color;
        }
    }
}
