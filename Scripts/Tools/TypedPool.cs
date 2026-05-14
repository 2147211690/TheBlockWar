namespace TheBlockWar.Scripts.Tools;

public class TypedPool<T> : IDisposable
{
    private readonly Func<T> _createFunc;
    private readonly Stack<T> _pool = new();
    public int PoolObjectCount => _pool.Count;

    public TypedPool(Func<T> createFunc, int initCount, Action<T>? beforeGet = null, Action<T>? afterReturn = null)
    {
        _createFunc = createFunc;
        BeforeGet = beforeGet;
        AfterReturn = afterReturn;
        for (var i = 0; i < initCount; i++) _pool.Push(_createFunc());
    }

    public Action<T>? BeforeGet { get; set; }
    public Action<T>? AfterReturn { get; set; }

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

    public virtual T Get()
    {
        var item = _pool.Count > 0 ? _pool.Pop() : _createFunc();
        BeforeGet?.Invoke(item);
        return item;
    }

    public virtual void Return(T item)
    {
        if (item is null) return;
        _pool.Push(item);
        AfterReturn?.Invoke(item);
    }
}