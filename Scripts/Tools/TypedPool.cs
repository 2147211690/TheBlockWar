using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Tools;

public class TypedPool<T> : IDisposable
{
    private readonly Func<T> _createFunc;
    private readonly Stack<T> _pool = new();

    public TypedPool(Func<T> createFunc, int initCount, Action<T>? beforePop = null, Action<T>? afterPush = null)
    {
        _createFunc = createFunc;
        BeforePop = beforePop;
        AfterPush = afterPush;
        for (var i = 0; i < initCount; i++) _pool.Push(_createFunc());
    }

    public Action<T>? BeforePop { get; set; }
    public Action<T>? AfterPush { get; set; }

    public void Dispose()
    {
        if (typeof(Node).IsAssignableFrom(typeof(T)))
        {
            foreach (var item in _pool)
                if (item is Node node)
                    node.Free();
        }
        else if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
        {
            foreach (var item in _pool) ((IDisposable)item).Dispose();
        }

        _pool.Clear();
    }

    public T Pop()
    {
        T item;
        if (_pool.Count > 0)
        {
            item = _pool.Pop();
            BeforePop?.Invoke(item);
        }
        else
        {
            item = _createFunc();
        }

        return item;
    }

    public void Push(T item)
    {
        _pool.Push(item);
        AfterPush?.Invoke(item);
    }
}