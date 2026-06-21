using System;
using System.Threading;

namespace TheBlockWar.Scripts.Tools;

/// <summary>
/// 异步任务队列，确保任务按 FIFO 顺序串行执行，线程安全。
/// </summary>
public class TaskQueue
{
    public event EventHandler<bool>? IsRunningChanged;

    public bool IsRunning
    {
        get;
        private set
        {
            if (field == value) return;
            field = value;
            IsRunningChanged?.Invoke(this, field);
        }
    }

    private readonly Queue<TaskItem> _queue = [];
    private readonly Lock _lock = new();
    private bool _isClearing;
    /// <summary>
    /// 添加任务并等待其完成。
    /// </summary>
    public GDTask AddTaskAndWaitAsync(Func<GDTask> taskFunc)
    {
        ArgumentNullException.ThrowIfNull(taskFunc);

        var completion = new GDTaskCompletionSource();
        EnqueueItem(new TaskItem(taskFunc, completion));
        return completion.Task;
    }

    /// <summary>
    /// 添加任务（不等待完成）。
    /// </summary>
    public void AddTask(Func<GDTask> taskFunc)
    {
        ArgumentNullException.ThrowIfNull(taskFunc);

        EnqueueItem(new TaskItem(taskFunc, null));
    }

    private void EnqueueItem(in TaskItem item)
    {
        bool needStart;
        lock (_lock)
        {
            _queue.Enqueue(item);
            needStart = !IsRunning;
            if (needStart)
            {
                IsRunning = true;
            }
        }

        if (needStart)
            ProcessLoopCore().Forget();
    }

    private async GDTask ProcessLoopCore()
    {
        while (true)
        {
            TaskItem item;
            lock (_lock)
            {
                item = _queue.Dequeue();
            }

            try
            {
                await item.TaskFunc();
                item.Completion?.TrySetResult();
            }
            catch (Exception ex)
            {
                // 异常隔离：单个任务失败不影响队列中其他任务
                item.Completion?.TrySetException(ex);
            }

            lock (_lock)
            {
                if (_isClearing)
                {
                    foreach (var a in _queue) a.Completion?.TrySetResult();
                    _queue.Clear();
                    _isClearing = false;
                    return;
                }
                
                if (_queue.Count > 0)
                    continue; // 还有任务，继续处理

                IsRunning = false;
                return;       // 队列为空，结束循环
            }
        }
    }

    public void Clear()
    {
        if (!IsRunning) return;
        lock (_lock)
        {
            _isClearing = true;
        }
    }
    
    /// <summary>
    /// 队列内部条目，用于统一 "有返回" 和 "无返回" 两种添加方式。
    /// </summary>
    private readonly struct TaskItem(Func<GDTask> taskFunc, GDTaskCompletionSource? completion)
    {
        public Func<GDTask> TaskFunc { get; } = taskFunc;
        public GDTaskCompletionSource? Completion { get; } = completion;
    }
}
