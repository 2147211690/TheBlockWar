using System;

namespace TheBlockWar.Scripts.Games;

public interface ISchedulerEvnet : IComparable<ISchedulerEvnet>
{
    uint Id { get; }
    /// <summary>
    /// 原则上不要修改它,除非你知道自己在做什么
    /// </summary>
    SimulationLayer Layer { get; set; }
    int Priority { get; set; }
    object? Args { get; set; }
    EventAction? Action { get; set; }
    void UpdateEvent();
    event EventHandler? Update;
    /// <summary>
    /// 运行事件,将事件层设置为Running
    /// </summary>
    void Run();
    public delegate void EventAction(IGameState state, object? args, SimulationResult result);
}