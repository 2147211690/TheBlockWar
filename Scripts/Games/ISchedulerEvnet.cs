using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public interface ISchedulerEvnet : IComparable<ISchedulerEvnet>
{
    uint Id { get; }
    SimulationLayer Layer { get; set; }
    int Team { get; set; }
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