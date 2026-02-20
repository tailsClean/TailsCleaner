using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel/Void", order = 0)]
public class VoidEventChannelSO : ScriptableObject
{
    private event Action _eventChannel;
    public void AddListener(Action action) => _eventChannel += action;
    public void RemoveListener(Action action) => _eventChannel -= action;

    public void OnStartEvent() => _eventChannel?.Invoke();
}
