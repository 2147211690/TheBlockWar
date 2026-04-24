using System.Linq;
using NLog;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class GameScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<GameScheduler> _runningSchedulers = new();
    
    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static GameScheduler? CurrentScheduler => _runningSchedulers.Peek();
    public ICollection<SchedulerEvent> RunningEvents => _eventList.RunningEvents;
    public ICollection<SchedulerEvent> FocusedEvents => _eventList.FocusedEvents;
    public ICollection<SchedulerEvent> WaitingEvents => _eventList.WaitingEvents;
    public IReadOnlyDictionary<long, SkillSchedulerEvent> SkillEvents => _eventList.SkillEvents;
    public IReadOnlyGameState? GameState => _gameState;
    public long FocusedUnitId { get; private set; }
    public int PlayerTeam { get; private set; }
    public IReadOnlyDictionary<long, (HashSet<Vector2I> Coords, int type)> CoordsRegions => _coordsRegions;
    
    private SchedulerEventList _eventList = new();
    
    private IGameState? _gameState;
    private Dictionary<long, (HashSet<Vector2I> Coords, int type)> _coordsRegions = new();
    private long _coordsRegionsId;
    private IGameState? _snapshotChache;
    
    public event EventHandler<List<SimulationContext>>? SimulationUpdated;
    public event EventHandler<OtherStateUpdateTypeEnum>? OtherStateUpdated;
    public event EventHandler<int>? NextDelay;
    
    private bool _isRunning = false;
    
    public GameScheduler()
    {
        _eventList.ListUpdated += OnEventListUpdate;
    }

    private void OnEventListUpdate(object? sender, EventArgs e)
    {
        UpdateSimulation();
    }

    public void InitScheduler(IGameState gameState, int playerTeam)
    { 
        _gameState = gameState;
        PlayerTeam = playerTeam;
    }
    public void FocusUnit(long id)
    {
        if (FocusedUnitId == id) return;
        FocusedUnitId = id;
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.FocusedUnitId);
    }

    public void SetNextCompleteDelay(int delay)
    {
        NextDelay?.Invoke(this, delay);
    }
    public SkillSchedulerEvent? GetUnitSkillEvent(long unitId)
    {
        _eventList.SkillEvents.TryGetValue(unitId, out var skillEvent);
        return skillEvent;
    }
    public SchedulerEvent AddEvent(SchedulerEvent ev, SimulationLayer? layer = null)
    {
        _eventList.AddEvent(ev, layer);
        return ev;
    }
    public void AddEventMany(IEnumerable<SchedulerEvent> ev, SimulationLayer? layer = null)
    {
        _eventList.AddEventMany(ev, layer);
    }
    public void RemoveEventMany(IEnumerable<SchedulerEvent> ev)
    {
        _eventList.RemoveEventMany(ev);
    }
    public void RemoveEvent(SchedulerEvent ev)
    {
        _eventList.RemoveEvent(ev);
    }
    public void BlockUpdate()
    {
        _eventList.BlockUpdate();
    }
    public void UnblockUpdate()
    {
        _eventList.UnblockUpdate();
    }
    public long AddCoordsRegion(IEnumerable<Vector2I> coords, int type)
    {
        var id = _coordsRegionsId++;
        _coordsRegions[id] = (coords.ToHashSet(), type);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
        return id;
    }
    public bool IsInCoordsRegion(long id,in Vector2I coord) =>
        CoordsRegions.TryGetValue(id, out var region) && region.Coords.Contains(coord);
    public void RemoveCoordsRegion(long id)
    {
        _coordsRegions.Remove(id);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
    }

    private void UpdateSimulation()
    {
        if (_gameState is null) return;
        if (_isRunning) return;
        _isRunning = true;
        Logger.Info("触发更新");
        var contexts = new List<SimulationContext>();
        _runningSchedulers.Push(this);
        
        // 运行层直接执行
        foreach (var ev in _eventList.RunningEvents)
        {
            var context = CreateSimulationContext(ev);
            ev.OnApply(ev, _gameState, context);
            contexts.Add(context);
        }
        // 运行层事件执行完毕后，清空运行层
        _eventList.ClearLayerEvent(SimulationLayer.Running);
        // 创建快照，用于焦点层和等待层事件执行
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState) ? _gameState.GetSnapshot() : _snapshotChache;
        snapshot.ClearIfSnapshot();
        // 焦点层和等待层事件执行
        foreach (var ev in _eventList.FocusedEvents.Concat( _eventList.WaitingEvents))
        {
            var context = CreateSimulationContext(ev);
            ev.OnApply(ev, snapshot, context);
            contexts.Add(context);
        }
        Logger.Debug("更新结果:\n{results}", string.Join("\n", contexts));
        _isRunning = false;
        SimulationUpdated?.Invoke(this, contexts);
        _runningSchedulers.Pop();
    }

    private SimulationContext CreateSimulationContext(SchedulerEvent ev)
    {
        var context = new SimulationContext()
        {
            SourceId = ev.Id,
            Layer = ev.Layer,
            TeamType = GameState!.TeamState.GetTeamTypeRelativeToPlayer(PlayerTeam, ev.Team),
            SourceUnitId = (ev as SkillSchedulerEvent)?.BindUnitId ?? 0,
        };
        return context;
    }

    public enum OtherStateUpdateTypeEnum
    {
        None,
        FocusedUnitId,
        CoordsRegion,
    }
}