using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

[CreateAssetMenu(fileName = "AsyncEventChannel", menuName = "EventChannel/AsyncEventChannel")]
public class AsyncEventChannelSO : ScriptableObject
{
    private readonly SortedList<int, List<Func<Task>>> _priorityListeners = new();
    private event Func<Task> _eventChannel;

    public void AddListener(Func<Task> action) => _eventChannel += action;
    public void RemoveListener(Func<Task> action) => _eventChannel -= action;

    // 낮은 숫자가 높은 우선순위
    public void AddPriorityListener(Func<Task> action, int priority)
    {
        if (!_priorityListeners.ContainsKey(priority))
            _priorityListeners.Add(priority, new List<Func<Task>>());

        _priorityListeners[priority].Add(action);
    }

    public void RemovePriorityListener(Func<Task> action, int priority)
    {
        if (_priorityListeners.TryGetValue(priority, out var listeners))
        {
            listeners.Remove(action);
            if (listeners.Count == 0)
                _priorityListeners.Remove(priority);
        }
    }

    public async Task OnStartEvent()
    {
        foreach (var list in _priorityListeners.Values)
            foreach (var listener in list)
                if (listener != null)
                    await listener.Invoke();

        if (_eventChannel != null)
            await _eventChannel.Invoke();
    }
}
