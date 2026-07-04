using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public interface IEventScheduler
{
    ICollection<SchedulerEvent> RunningEvents { get; }
    ICollection<SchedulerEvent> FocusedEvents { get; }
    ICollection<SchedulerEvent> WaitingEvents { get; }
    ICollection<SchedulerEvent> DisabledEvents { get; }
    IReadOnlyDictionary<long, SchedulerEvent> SkillEvents { get; }
    IReadOnlyDictionary<long, SchedulerEvent> Events { get; }
    IReadOnlyGameState GameState { get; }
    int PlayerTeam { get; }
    int CurrentTurn { get; set; }
    bool IsRunning { get; set; }
    event EventHandler<SchedulerResult>? SimulationUpdated;
    event EventHandler<IReadOnlyGameState>? GameStateReload;
    void ReloadGameState();
    SchedulerEvent? GetUnitSkillEvent(long unitId);
    T CreateEvent<T>(Func<long, T> createFunc) where T : SchedulerEvent;
    void DeleteEvent(SchedulerEvent ev);
    void DeleteEvent(long id);
    void BlockUpdate();
    void UnblockUpdate();
    IReadOnlyList<SchedulerEvent> GetTurnEvents(int turn);
    IReadOnlyList<SchedulerEvent> GetCoordsEvents(Vector2I coords);
    GDTask WaitCompleteRunning();

    /// <summary>
    /// 更新模拟
    /// </summary>
    void UpdateSimulation();
}