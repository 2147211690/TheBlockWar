namespace TheBlockWar.Scripts.Tools;

public class RecordTypedPool<T> : TypedPool<T>
{
    public IReadOnlyList<T> PopedItems => _popedItems;
    private List<T> _popedItems = new();
    public RecordTypedPool(Func<T> createFunc, int initCount, Action<T>? beforeGet = null, Action<T>? afterReturn = null) : base(createFunc, initCount, beforeGet, afterReturn)
    {
    }

    public override T Get()
    {
        var item = base.Get();
        _popedItems.Add(item);
        return item;
    }

    public override void Return(T item)
    {
        base.Return(item);
        _popedItems.Remove(item);
    }

    public void PushAllPopedItems()
    {
        foreach (var item in _popedItems)
        {
            base.Return(item);
        }
        _popedItems.Clear();
    }
}