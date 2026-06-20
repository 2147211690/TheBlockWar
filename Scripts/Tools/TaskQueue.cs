using System.Threading;

namespace TheBlockWar.Scripts.Tools;
public class TaskQueue
{
    public EventHandler<bool>? IsRunningChanged;
    public bool IsRunning
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            IsRunningChanged?.Invoke(this, value);
        }
    }
    private readonly Queue<Func<GDTask>> _queue = [];
    private Lock _queueLoock = new();
    
    /// <summary>
    /// 添加一个事件,他会等待之前添加的事件执行完成之后再执行
    /// </summary>
    /// <param name="taskFunc">事件函数</param>
    /// <returns>当前事件确实执行完成任务</returns>
    public GDTask AddTaskAndWaitAsync(Func<GDTask> taskFunc)
    {
        var completionSource = new GDTaskCompletionSource();
        lock(_queueLoock)
        {
            _queue.Enqueue(CreateTask(taskFunc, completionSource));
            if (_queue.Count == 1)
            {
                IsRunning = true;
                _queue.Peek()().Forget(); // 如果是第一个,直接执行
            }
        }
        return completionSource.Task;

        Func<GDTask> CreateTask(Func<GDTask> func, GDTaskCompletionSource completion)
        {
            return async () =>
            {
                await func();
                lock(_queueLoock)
                {
                    _queue.Dequeue(); // 去除自己
                    completion.TrySetResult();
                    if (_queue.Count == 0) IsRunning = false;
                    else _queue.Peek()().Forget(); // 执行下一个
                }
            };
        }
    }
    /// <summary>
    /// 添加一个事件,他会等待之前添加的事件执行完成之后再执行
    /// </summary>
    /// <param name="taskFunc">事件函数</param>
    public void AddTask(Func<GDTask> taskFunc)
    {
        lock(_queueLoock)
        {
            _queue.Enqueue(CreateTask(taskFunc));
            if (_queue.Count == 1) 
            {
                IsRunning = true;
                _queue.Peek()().Forget(); // 如果是第一个,直接执行
            }
        }

        Func<GDTask> CreateTask(Func<GDTask> func)
        {
            return async () =>
            {
                await func();
                lock(_queueLoock)
                {
                    _queue.Dequeue(); // 去除自己
                    if (_queue.Count == 0) IsRunning = false;
                    else _queue.Peek()().Forget(); // 执行下一个
                }
            };
        }
    }
}