using System;
using UnityEngine;

public class EventChannelBase<T> : ScriptableObject
{
    private event Action<T> _eventChannel;

    public void AddListener(Action<T> action) => _eventChannel += action;
    public void RemoveListener(Action<T> action) => _eventChannel -= action;

    public void OnStartEvent(T value) => _eventChannel?.Invoke(value);
}