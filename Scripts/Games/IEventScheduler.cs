using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public interface IEventScheduler
{
    IReadOnlyGameState GameState { get; }
    int PlayerTeam { get; }
    int CurrentTurn { get; set; }
    Dictionary<int, Dictionary<string, int>> PhaseOrders { get; }
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
    EventState CreateEvent(EventCore core, int phaseOrder, int team, SimulationLayer layer, int priority = 0,
        EventAfterRunMode afterRunMode = EventAfterRunMode.Remove);
    void RemoveEvent(long id);
    
    /// <summary>
    /// 更新模拟
    /// </summary>
    void UpdateSimulation();
    
    /// <summary>
    /// 设置单位将要使用的技能
    /// </summary>
    void PrepareUnitSkill(long unitId, SkillPrepareInfo? skillPrepareInfo);
    /// <summary>
    /// 设置环境事件将要执行的操作
    /// </summary>
    ApplyStep PrepareEnv(Vector2I coords, ApplyStep step);
    /// <summary>
    /// 设置团队将要放置的单位
    /// </summary>
    void PrepareTeamPlaceUnit(Vector2I coords, int team, int index, TeamPreparePlaceUnitInfo? placeUnitInfo);
    
    Vector2I? FocusedEnvCoords { get; set; }
    long FocusedUnitId { get; set; }
    Vector2I? FocusedTileCoords { get; set; }
    EventFocusedModeMask FocusedMode { get; set; }

    void RunBuffs();
    void RunRegions();
    void RunEnvs();
    void RunSkills(int team);
    void RunSkill(long unitId);
    void RunTeamPlaceUnit(int team);
}

[Flags]
public enum EventFocusedModeMask
{
    None = 0,
    SelectedUnit = 1 << 0,
    SelectedTile = 1 << 1,
    SelectedCoordsEnv = 1 << 2,
    AllRegion = 1 << 3,
    AllBuff = 1 << 4,
    AllUnit = 1 << 5,
    AllTile = 1 << 6,
    AllEnvt = 1 << 7,
    All = SelectedUnit | SelectedTile | SelectedCoordsEnv | AllRegion | AllBuff | AllUnit | AllTile | AllEnvt,
}