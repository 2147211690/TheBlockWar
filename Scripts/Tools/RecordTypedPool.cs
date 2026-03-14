using System;
using System.Collections.Generic;

namespace TheBlockWar.Scripts.Tools;

public class RecordTypedPool<T> : TypedPool<T>
{
    public IReadOnlyList<T> PopedItems => _popedItems;
    private List<T> _popedItems = new();
    public RecordTypedPool(Func<T> createFunc, int initCount, Action<T>? beforePop = null, Action<T>? afterPush = null) : base(createFunc, initCount, beforePop, afterPush)
    {
    }

    public override T Pop()
    {
        var item = base.Pop();
        _popedItems.Add(item);
        return item;
    }

    public override void Push(T item)
    {
        base.Push(item);
        _popedItems.Remove(item);
    }

    public void PushAllPopedItems()
    {
        foreach (var item in _popedItems)
        {
            base.Push(item);
        }
        _popedItems.Clear();
    }
}