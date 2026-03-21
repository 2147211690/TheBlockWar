using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Tools;
public partial class CallHelper : Node
{
    private Queue<(object,Action<object>)> _callActions = new();
    private HashSet<object> _calledKeys = new();
    public override void _Process(double delta)
    {
        if (_callActions.Count == 0) return;
        var count = _callActions.Count;
        _calledKeys.Clear();
        while (count-- > 0)
        {
            var pair = _callActions.Dequeue();
            pair.Item2(pair.Item1);
        }
    }
    public void QueueCall(object key, Action<object> action)
    {
        if (_calledKeys.Contains(key)) return;
        _callActions.Enqueue((key,action));
        _calledKeys.Add(key);
    }
    
}