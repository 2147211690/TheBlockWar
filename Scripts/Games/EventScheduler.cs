using System.Linq;
using System.Text;
using System.Text.Json;
using NLog;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Tools.Jsons;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class EventScheduler : IEventScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<IEventScheduler> _runningSchedulers = new();

    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static IEventScheduler CurrentScheduler => _runningSchedulers.Peek();

    public ICollection<SchedulerEvent> RunningEvents => _eventManager.RunningEvents;
    public ICollection<SchedulerEvent> FocusedEvents => _eventManager.FocusedEvents;
    public ICollection<SchedulerEvent> WaitingEvents => _eventManager.WaitingEvents;
    public ICollection<SchedulerEvent> DisabledEvents => _eventManager.DisabledEvents;
    public IReadOnlyDictionary<long, SchedulerEvent> SkillEvents => _eventManager.SkillEvents;
    public IReadOnlyDictionary<long, SchedulerEvent> Events => _eventManager.Events;
    public IReadOnlyGameState GameState => _gameState;
    public int PlayerTeam { get; }
    public int CurrentTurn { get => _gameState.TurnState.CurrentTurn; set => _gameState.TurnState.CurrentTurn = value; }
    private SchedulerEventManager _eventManager;

    private IGameState _gameState;
    private IGameState? _snapshotChache;
    public event EventHandler<SchedulerResult>? SimulationUpdated;
    public event EventHandler<IReadOnlyGameState>? GameStateReload;

    public bool IsRunning
    {
        get;
        set
        { 
            if (field == value) return;
            field = value;
            if (field)
            {
                _runningCompletionSource = new();
                _runningCompletionSource.Task.Forget();
            }
            else
            {
                _runningCompletionSource!.TrySetResult();
                _runningCompletionSource = null;
            }
        }
    }
    private GDTaskCompletionSource? _runningCompletionSource;

    private bool _isSimulating = false;
    private IEventScheduler _eventSchedulerImplementation;

    public EventScheduler(IGameState gameState, int playerTeam)
    {
        _gameState = gameState;
        PlayerTeam = playerTeam;
        _eventManager = new(this, () => _gameState.Events);
        _eventManager.EnabledListUpdated += OnEventEnabledManagerUpdate;
    }

    private void OnEventEnabledManagerUpdate(object? sender, EventArgs e)
    {
        UpdateSimulation();
    }
    public void ReloadGameState()
    {
        GameStateReload?.Invoke(this, _gameState);
        _eventManager.ReloadStateEvents();
        _snapshotChache = null;
    }
    
    public SchedulerEvent? GetUnitSkillEvent(long unitId)
    {
        SkillEvents.TryGetValue(unitId, out var skillEvent);
        return skillEvent;
    }
    
    public T CreateEvent<T>(Func<long, T> createFunc) where T : SchedulerEvent
    {
        return _eventManager.CreateEvent(createFunc);
    }
    public void DeleteEvent(SchedulerEvent ev)
    {
        _eventManager.DeleteEvent(ev);
    }
    public void DeleteEvent(long id)
    {
        _eventManager.DeleteEvent(id);
    }

    public void BlockUpdate()
    {
        _eventManager.BlockUpdate();
    }

    public void UnblockUpdate()
    {
        _eventManager.UnblockUpdate();
    }
    public IReadOnlyList<SchedulerEvent> GetTurnEvents(int turn) => _eventManager.GetTurnEvents(turn);
    public IReadOnlyList<SchedulerEvent> GetCoordsEvents(Vector2I coords) => _eventManager.GetCoordsEvents(coords);

    public GDTask WaitCompleteRunning() => _runningCompletionSource?.Task ?? GDTask.CompletedTask;
    /// <summary>
    /// 更新模拟
    /// </summary>
    public void UpdateSimulation()
    {
        if (_isSimulating) return;
        _isSimulating = true;
        Logger.Info("触发更新");
        var result = new SchedulerResult();
        _runningSchedulers.Push(this);

        // 运行层直接执行
        foreach (var ev in _eventManager.RunningEvents)
        {
            if (!ev.CheckIsValid(GameState)) continue;
            var obj = new SimObject(_gameState, ev.Layer, new(), ev.Id);
            obj.PushContext(CreateSimulationContext(ev));
            result.Sims.Add(obj);
            ev.OnApply(ev, obj);
            obj.PopContext();
        }
        result.RunningCount = _eventManager.RunningEvents.Count;
        // 创建快照，用于焦点层和等待层事件执行
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState)
                ? _gameState.GetSnapshot()
                : _snapshotChache;
        snapshot.ClearIfSnapshot();
        // 焦点层和等待层事件执行
        foreach (var ev in _eventManager.FocusedEvents.Concat(_eventManager.WaitingEvents))
        {
            if (!ev.CheckIsValid(snapshot)) continue;
            var obj = new SimObject(_snapshotChache, ev.Layer, new(), ev.Id);
            obj.PushContext(CreateSimulationContext(ev));
            result.Sims.Add(obj);
            ev.OnApply(ev, obj);
            obj.PopContext();
        }
        
        var sb = new StringBuilder();
        //Logger.Trace("结构树:\n{tree}", result.Steps.ToTree());
        // 运行层事件执行完毕后，清空运行层
        _eventManager.ClearLayerEvent(SimulationLayer.Running);
        
        _runningSchedulers.Pop();
        _isSimulating = false;
        SimulationUpdated?.Invoke(this, result);
        var json = result.ToJson();
        var r = GameJson.FromJson<SchedulerResult>(json);
    }

    /// <summary>
    /// 更新模拟
    /// </summary>
    public void UpdateSimulationTest()
    {
        if (_isSimulating) return;
        _isSimulating = true;
        Logger.Info("触发更新");
        var result = new SchedulerResult();
        _runningSchedulers.Push(this);
        
        result.RunningCount = _eventManager.RunningEvents.Count;
        // 创建快照，用于焦点层和等待层事件执行
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState)
                ? _gameState.GetSnapshot()
                : _snapshotChache;
        snapshot.ClearIfSnapshot();

        // 焦点层和等待层事件执行
        foreach (var ev in _eventManager.RunningEvents.Concat(_eventManager.FocusedEvents.Concat(_eventManager.WaitingEvents)))
        {
            if (!ev.CheckIsValid(snapshot)) continue;
            var obj = new SimObject(_snapshotChache, ev.Layer, new(), ev.Id);
            obj.PushContext(CreateSimulationContext(ev));
            result.Sims.Add(obj);
            ev.OnApply(ev, obj);
            obj.PopContext();
        }
        
        var sb = new StringBuilder();
        //Logger.Trace("结构树:\n{tree}", result.Steps.ToTree());
        // 运行层事件执行完毕后，清空运行层
        _eventManager.ClearLayerEvent(SimulationLayer.Running);
        
        _runningSchedulers.Pop();
        _isSimulating = false;
        
        var json = result.ToJson();
        var r = GameJson.FromJson<SchedulerResult>(json);
        r?.Replay(_gameState);
        SimulationUpdated?.Invoke(this, result);
    }
    
    
    private SimContext CreateSimulationContext(SchedulerEvent ev)
    {
        var context = new SimContext(ev is SkillEvent sev ? sev.SkillBindUnitId : 0);
        return context;
    }
}