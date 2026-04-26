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

    public virtual void Dispose()
    {
        if (typeof(Node).IsAssignableFrom(typeof(T)))
        {
            foreach (var item in _pool)
                if (item is Node node)
                    node.QueueFree();
        }
        else if (typeof(IDisposable).IsAssignableFrom(typeof(T)))
        {
            foreach (var item in _pool) ((IDisposable)item!).Dispose();
        }

        _pool.Clear();
    }

    public virtual T Pop()
    {
        var item = _pool.Count > 0 ? _pool.Pop() : _createFunc();
        BeforePop?.Invoke(item);
        return item;
    }

    public virtual void Push(T item)
    {
        if (item is null) return;
        _pool.Push(item);
        AfterPush?.Invoke(item);
    }
}