using System;
using System.Collections.Generic;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 事件类,用于游戏调度器
/// 事件越小,优先级越高
/// </summary>
public class SchedulerEvnet : ISchedulerEvnet
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public SchedulerEvnet(uint id)
    {
        Id = id;
    }
    public uint Id { get; }
    //TODO:可能在修改时不通知,因为他只能由调度器管理
    /// <summary>
    /// 原则上不要修改它,除非你知道自己在做什么
    /// </summary>
    SimulationLayer ISchedulerEvnet.Layer
    {
        get => field;
        set => SetField(ref field, value);
    } = SimulationLayer.Waiting;

    public int Team { get => field; set => SetField(ref field, value); }
    public SimulationLayer Layer => ((ISchedulerEvnet)this).Layer;
    public int Priority { get => field; set => SetField(ref field, value); }
    public object? Args { get => field; set => SetField(ref field, value, false); }
    public ISchedulerEvnet.EventAction? Action { get => field; set => SetField(ref field, value, false); }
    public void UpdateEvent() => Update?.Invoke(this, EventArgs.Empty);
    protected void SetField<T>(ref T field, in T value, bool checkEqual = true)
    {
        if (checkEqual && EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Update?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler? Update;
    
    //TODO:可能分离与Layer的绑定,使用单独的事件通知
    /// <summary>
    /// 运行事件,将事件层设置为Running
    /// </summary>
    public void Run() =>
        ((ISchedulerEvnet)this).Layer = SimulationLayer.Running;
    public static bool operator <(SchedulerEvnet a, SchedulerEvnet b) => a.CompareTo(b) < 0;
    public static bool operator >(SchedulerEvnet a, SchedulerEvnet b) => a.CompareTo(b) > 0;
    public int CompareTo(ISchedulerEvnet? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (Layer != other.Layer) return Layer.CompareTo(other.Layer);
        return Priority.CompareTo(other.Priority);
    }
    
    public override string ToString() => $"事件ID:{Id} 事件层:{Layer} 事件优先级:{Priority} 事件参数类型:{Args?.GetType().Name}";
}