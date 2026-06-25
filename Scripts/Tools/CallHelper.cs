namespace TheBlockWar.Scripts.Tools;
public partial class CallHelper : Node
{
    private Queue<(object,Action<object>)> _callActions = new();
    private HashSet<object> _calledKeys = new();
    private static CallHelper _instance;

    protected CallHelper()
    {
        if (_instance != null)
        {
            QueueFree();
        }
        else _instance = this;
    }
    
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
    public static void QueueCall(object caller, Action<object> action)
    {
        if (_instance._calledKeys.Contains(caller)) return;
        _instance._callActions.Enqueue((caller,action));
        _instance._calledKeys.Add(caller);
    }
    
}