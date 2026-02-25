using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EventChannel", menuName = "EventChannel/Void", order = 0)]
public class VoidEventChannelSO : ScriptableObject
{
    private readonly SortedList<int, List<Action>> _priorityListeners = new SortedList<int, List<Action>>();
    private event Action _eventChannel;
    public void AddListener(Action action) => _eventChannel += action;
    
    //우선 순위가 높은 리스너가 먼저 실행되도록 하는 메서드 (낮은 숫자가 높은 우선 순위를 나타냄)
    public void AddPriorityListener(Action action, int priority)
    {
        _priorityListeners.Add(priority, new List<Action>());
       
       _priorityListeners[priority].Add(action);
    }
    
    public void RemoveListener(Action action) => _eventChannel -= action;
    public void RemovePriorityListener(Action action, int priority)
    {
        if(_priorityListeners.TryGetValue(priority, out var listeners))
        {
            listeners.Remove(action);
            
            if(listeners.Count == 0)
                _priorityListeners.Remove(priority);
        }
    }
    public void OnStartEvent()
    {
        foreach(var list in _priorityListeners.Values)
        {
            foreach(var listener in list)
                listener?.Invoke(); 
        }
        _eventChannel?.Invoke();
    }

    
}
