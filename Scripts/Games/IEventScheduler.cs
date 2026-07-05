using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public interface IEventScheduler
{
    IReadOnlyGameState GameState { get; }
    int PlayerTeam { get; }
    int CurrentTurn { get; set; }
    bool IsRunning { get; set; }
    event EventHandler<SchedulerResult>? SimulationUpdated;
    event EventHandler<IReadOnlyGameState>? GameStateReload;
    void ReloadGameState();
    void BlockUpdate();
    void UnblockUpdate();
    GDTask WaitCompleteRunning();

    IReadOnlyDictionary<long, EventState> Events { get; }
    IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords);
    IReadOnlyList<EventState> GetUnitEvents(long unitId);
    IReadOnlyList<EventState> GetTeamEvents(int team);
    IReadOnlyList<EventState> GetTurnEvents(int turn);
    EventState CreateEvent(EventCore core, int turn, int phaseOrder, int team, SimulationLayer layer, int priority = 0);
    void RemoveEvent(long id);
    
    /// <summary>
    /// 更新模拟
    /// </summary>
    void UpdateSimulation();
}