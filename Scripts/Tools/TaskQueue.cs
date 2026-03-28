namespace TheBlockWar.Scripts.Tools;
public class TaskQueue
{
    private readonly Queue<Func<GDTask>> _queue = [];
    /// <summary>
    /// 添加一个事件,他会等待之前添加的事件执行完成之后再执行
    /// </summary>
    public GDTask AddTask(Func<GDTask> taskFunc)
    {
        var completionSource = new GDTaskCompletionSource();
        _queue.Enqueue(CreateTask(taskFunc, completionSource));
        if (_queue.Count == 1) _queue.Peek()().Forget(); // 如果是第一个,直接执行
        return completionSource.Task;

        Func<GDTask> CreateTask(Func<GDTask> func, GDTaskCompletionSource completion)
        {
            return async () =>
            {
                await func();
                _queue.Dequeue(); // 去除自己
                completion.TrySetResult();
                if (_queue.Count != 0) _queue.Peek()().Forget(); // 执行下一个
            };
        }
    }
}