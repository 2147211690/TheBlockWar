namespace TheBlockWar.Scripts.Tools;
public class TaskQueue
{
    private List<GDTaskCompletionSource> _queue = [];
    /// <summary>
    /// 添加一个事件,他会等待之前添加的事件执行完成之后再执行
    /// </summary>
    public GDTask AddTask(Func<GDTask> taskFunc)
    {
        var completionSource = new GDTaskCompletionSource();
        var t = CreateTask(_queue.Count, taskFunc, completionSource);
        _queue.Add(completionSource);
        return t;

        async GDTask CreateTask(int index, Func<GDTask> func, GDTaskCompletionSource completion)
        {
            if (index != 0) await _queue[index - 1].Task;
            await func();
            completion.TrySetResult();
            if (index == _queue.Count - 1)
            {
                _queue[^1].Task.Forget();
                _queue.Clear();
            }
        }
    }
}