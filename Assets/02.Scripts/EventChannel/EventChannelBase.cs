using System;
using System.Collections.Generic;
using UnityEngine;

public class EventChannelBase<T> : ScriptableObject
{
    private readonly SortedList<int, List<Action<T>>> _priorityListeners = new SortedList<int, List<Action<T>>>();
    private event Action<T> _eventChannel;

    public void AddListener(Action<T> action) => _eventChannel += action;

    //우선 순위가 높은 리스너가 먼저 실행되도록 하는 메서드 (낮은 숫자가 높은 우선 순위를 나타냄)
    public void AddPriorityListener(Action<T> action, int priority)
    {
        if(!_priorityListeners.ContainsKey(priority))
        {
            _priorityListeners.Add(priority, new List<Action<T>>());
        }

        _priorityListeners[priority].Add(action);
        
    }
    
    public void RemoveListener(Action<T> action) => _eventChannel -= action;

    public void RemovePriorityListener(Action<T> action, int priority)
    {
        if(_priorityListeners.TryGetValue(priority, out var listeners))
        {
            listeners.Remove(action);
            
            if(listeners.Count == 0)
                _priorityListeners.Remove(priority);
        }
    }
    public void OnStartEvent(T value)
    {
        foreach(var list in _priorityListeners.Values)
        {
            foreach(var listener in list)
                listener?.Invoke(value); 
        }
        _eventChannel?.Invoke(value);
    }
}